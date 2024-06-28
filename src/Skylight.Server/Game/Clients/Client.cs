using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Net.Sockets;
using Skylight.API.Game.Clients;
using Skylight.API.Game.Users;
using Skylight.Protocol.Packets.Data.CallForHelp;
using Skylight.Protocol.Packets.Data.Perk;
using Skylight.Protocol.Packets.Outgoing;
using Skylight.Protocol.Packets.Outgoing.Availability;
using Skylight.Protocol.Packets.Outgoing.CallForHelp;
using Skylight.Protocol.Packets.Outgoing.Handshake;
using Skylight.Protocol.Packets.Outgoing.Navigator;
using Skylight.Protocol.Packets.Outgoing.Notifications;
using Skylight.Protocol.Packets.Outgoing.Perk;

namespace Skylight.Server.Game.Clients;

internal sealed class Client : IClient
{
	public ISocket Socket { get; }
	public Encoding Encoding { get; }

	public long LastPongReceived { get; set; }

	public IUser? User { get; private set; }

	private readonly PacketScheduler packetScheduler;

	internal Client(ISocket socket, Encoding encoding)
	{
		this.Socket = socket;
		this.Encoding = encoding;

		this.LastPongReceived = Environment.TickCount64;

		this.packetScheduler = new PacketScheduler(this);
	}

	public void Authenticate(IUser user)
	{
		if (this.User is not null)
		{
			throw new InvalidOperationException();
		}

		this.User = user;

		//SSO stuff
		this.SendAsync(new AuthenticationOKOutgoingPacket());
		//Avatar effects
		//Unseen items
		//Figure set ids
		this.SendAsync(new NoobnessLevelOutgoingPacket(0));
		this.SendAsync(new NavigatorSettingsOutgoingPacket(user.Settings.HomeRoomId, user.Settings.HomeRoomId));
		this.SendAsync(new UserRightsOutgoingPacket(7, 7, true));
		//Favourites
		this.SendAsync(new AvailabilityStatusOutgoingPacket(true, false, true));
		this.SendAsync(new InfoFeedEnableOutgoingPacket(true));
		//Activity points
		//Achievement score
		this.SendAsync(new IsFirstLoginOfDayOutgoingPacket(true));
		//Mystery box keys
		//Builder club sub
		this.SendAsync(new CfhTopicsInitOutgoingPacket(Array.Empty<CallForHelpCategoryData>()));

		this.SendAsync(new PerkAllowancesOutgoingPacket
		{
			Perks =
			[
				new PerkAllowanceData("USE_GUIDE_TOOL", string.Empty, true),
				new PerkAllowanceData("GIVE_GUIDE_TOURS", string.Empty, true),
				new PerkAllowanceData("JUDGE_CHAT_REVIEWS", string.Empty, true),
				new PerkAllowanceData("VOTE_IN_COMPETITIONS", string.Empty, true),
				new PerkAllowanceData("SAFE_CHAT", string.Empty, true),
				new PerkAllowanceData("FULL_CHAT", string.Empty, true),
				new PerkAllowanceData("CALL_ON_HELPERS", string.Empty, true),
				new PerkAllowanceData("CITIZEN", string.Empty, true),
				new PerkAllowanceData("TRADE", string.Empty, true),
				new PerkAllowanceData("BUILDER_AT_WORK", string.Empty, true),
				new PerkAllowanceData("CAMERA", string.Empty, true),
				new PerkAllowanceData("NAVIGATOR_ROOM_THUMBNAIL_CAMERA", string.Empty, true),
				new PerkAllowanceData("MOUSE_ZOOM", string.Empty, true),
				new PerkAllowanceData("NAVIGATOR_PHASE_ONE_2014", string.Empty, true),
				new PerkAllowanceData("NAVIGATOR_PHASE_TWO_2014", string.Empty, true)
			]
		});
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void SendAsync<T>(in T packet)
		where T : IGameOutgoingPacket
		=> this.Socket.SendAsync(packet).Preserve();

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool ScheduleTask<T>(in T task)
		where T : IClientTask
		=> this.packetScheduler.ScheduleTask(task);

	public bool ScheduleTask(Func<IClient, Task> task)
	{
#if !DEBUG
		throw new NotSupportedException();
#else
		return this.ScheduleTask(new FuncClientTaskWrapper(task));
#endif
	}

	public void Disconnect()
	{
		this.User?.Disconnect();
	}

	private sealed class PacketScheduler
	{
		private readonly Client client;

		private readonly Dictionary<Type, int> taskLimits;

		private readonly Dictionary<Type, object?> scheduledTasks;

		internal PacketScheduler(Client client)
		{
			this.client = client;

			this.taskLimits = [];

			this.scheduledTasks = [];
		}

		internal bool ScheduleTask<T>(in T task)
			where T : IClientTask
		{
			lock (this.scheduledTasks)
			{
				ref object? data = ref this.scheduledTasks.Count <= 0
					? ref Unsafe.NullRef<object?>()
					: ref CollectionsMarshal.GetValueRefOrNullRef(this.scheduledTasks, typeof(T));
				if (Unsafe.IsNullRef(ref data))
				{
					//We do not have any tasks of this type so we are safe to start executing immediately
					Task promise = task.ExecuteAsync(this.client);

					return promise.IsCompletedSuccessfully || this.ScheduleTaskPromise(typeof(T), promise, ContinuationHolder<T>.Continuation);
				}

				return this.ScheduleTaskSlow(ref data, task);
			}
		}

		private bool ScheduleTaskPromise(Type key, Task task, Action<Task, object?> continuation)
		{
			//Fast path if the task faulted
			if (task.IsFaulted)
			{
				throw task.Exception;
			}

			//Add entry but no value as in reserved for the next task
			CollectionsMarshal.GetValueRefOrAddDefault(this.scheduledTasks, key, out _);

			task.ContinueWith(continuation, this, TaskContinuationOptions.ExecuteSynchronously);

			return true;
		}

		private bool ScheduleTaskSlow(ref object? data, IClientTask task)
		{
			this.taskLimits[task.GetType()] = 400;

			if (this.taskLimits.TryGetValue(task.GetType(), out int limit) && limit > 0)
			{
				if (data == null)
				{
					//We can use the slot directly to hold one queued up task
					data = task;

					return true;
				}
				else if (data is IClientTask oldTask)
				{
					//If we allow more than one queued up task then we allocate a queue
					if (limit > 1)
					{
						Queue<IClientTask> queue = new();
						queue.Enqueue(oldTask);
						queue.Enqueue(task);

						data = queue;

						return true;
					}
				}
				else if (data is Queue<IClientTask> tasks)
				{
					if (limit > tasks.Count)
					{
						tasks.Enqueue(task);

						return true;
					}
				}
				else
				{
					Debug.Fail("Unknown object");
				}
			}

			return false;
		}

		private void ScheduledTaskCompletion(Type key, Task task, Action<Task, object?> continuation)
		{
			if (!task.IsCompletedSuccessfully)
			{
				this.client.Socket.Disconnect(task.Exception!);

				return;
			}

			lock (this.scheduledTasks)
			{
				ref object? data = ref CollectionsMarshal.GetValueRefOrNullRef(this.scheduledTasks, key);

				Debug.Assert(!Unsafe.IsNullRef(ref data));

				if (data == null)
				{
					this.scheduledTasks.Remove(key);
				}
				else if (data is IClientTask newTask)
				{
					data = null;
					newTask.ExecuteAsync(this.client).ContinueWith(continuation, this, TaskContinuationOptions.ExecuteSynchronously);
				}
				else if (data is Queue<IClientTask> tasks)
				{
					if (tasks.TryDequeue(out IClientTask? queuedTask))
					{
						queuedTask.ExecuteAsync(this.client).ContinueWith(continuation, this, TaskContinuationOptions.ExecuteSynchronously);
					}
					else
					{
						this.scheduledTasks.Remove(key);
					}
				}
				else
				{
					Debug.Fail("Unknown object");
				}
			}
		}

		private static class ContinuationHolder<T>
			where T : IClientTask
		{
			internal static Action<Task, object?> Continuation { get; set; } = (task, state) =>
			{
				Unsafe.As<PacketScheduler>(state!).ScheduledTaskCompletion(typeof(T), task, ContinuationHolder<T>.Continuation!);
			};
		}
	}

	private readonly struct FuncClientTaskWrapper(Func<IClient, Task> action) : IClientTask
	{
		public Task ExecuteAsync(IClient client) => action(client);
	}
}
