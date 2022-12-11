using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Net.Sockets;
using Skylight.API.Game.Clients;
using Skylight.API.Game.Users;
using Skylight.Protocol.Packets.Outgoing;

namespace Skylight.Server.Game.Clients;

internal sealed class Client : IClient
{
	public ISocket Socket { get; }

	public IUser? User { get; private set; }

	private readonly PacketScheduler packetScheduler;

	internal Client(ISocket socket)
	{
		this.Socket = socket;

		this.packetScheduler = new PacketScheduler(this);
	}

	public void Authenticate(IUser user)
	{
		if (this.User is not null)
		{
			throw new InvalidOperationException();
		}

		this.User = user;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void SendAsync<T>(in T packet)
		where T : IGameOutgoingPacket
		=> this.Socket.SendAsync(packet);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool ScheduleTask<T>(in T task)
		where T : IClientTask
		=> this.packetScheduler.ScheduleTask(task);

	public void Disconnect()
	{
		this.User?.Disconnect();
	}

	private sealed class PacketScheduler
	{
		private readonly Client client;

		private readonly Dictionary<Type, int> taskLimits;

		private readonly Dictionary<Type, ScheduleData> scheduledTasks;

		internal PacketScheduler(Client client)
		{
			this.client = client;

			this.taskLimits = new Dictionary<Type, int>();

			this.scheduledTasks = new Dictionary<Type, ScheduleData>();
		}

		internal bool ScheduleTask<T>(in T task)
			where T : IClientTask
		{
			lock (this.scheduledTasks)
			{
				ref ScheduleData data = ref CollectionsMarshal.GetValueRefOrAddDefault(this.scheduledTasks, typeof(T), out _);

				if (data.CurrentTask is null)
				{
					data.CurrentTask = task.ExecuteAsync(this.client);
					data.CurrentTask.ContinueWith(static (task, state) =>
					{
						Unsafe.As<PacketScheduler>(state!).ScheduledTaskCompletion<T>(task);
					}, this, TaskContinuationOptions.ExecuteSynchronously);

					return true;
				}

				return this.ScheduleTaskSlow(ref data, task);
			}
		}

		private bool ScheduleTaskSlow(ref ScheduleData data, IClientTask task)
		{
			if (this.taskLimits.TryGetValue(task.GetType(), out int limit))
			{
				int current = data.QueuedTasks?.Count ?? 0;
				if (limit > current)
				{
					data.QueuedTasks ??= new Queue<IClientTask>();
					data.QueuedTasks.Enqueue(task);

					return true;
				}
			}

			return false;
		}

		private void ScheduledTaskCompletion<T>(Task task)
			where T : IClientTask
		{
			if (task.IsCompletedSuccessfully)
			{
				lock (this.scheduledTasks)
				{
					ref ScheduleData data = ref CollectionsMarshal.GetValueRefOrNullRef(this.scheduledTasks, typeof(T));

					Debug.Assert(!Unsafe.IsNullRef(ref data));
					Debug.Assert(data.CurrentTask == task);

					Queue<IClientTask>? queuedTasks = data.QueuedTasks;
					if (queuedTasks is null)
					{
						this.scheduledTasks.Remove(typeof(T));
					}
					else if (queuedTasks.TryDequeue(out IClientTask? queuedTask))
					{
						data.CurrentTask = queuedTask.ExecuteAsync(this.client);
						data.CurrentTask.ContinueWith(static (task, state) =>
						{
							Unsafe.As<PacketScheduler>(state!).ScheduledTaskCompletion<T>(task);
						}, this, TaskContinuationOptions.ExecuteSynchronously);
					}
					else
					{
						Debug.Assert(queuedTasks.Count == 0);

						this.scheduledTasks.Remove(typeof(T));
					}
				}
			}
			else
			{
				this.client.Socket.Disconnect(task.Exception!);
			}
		}

		private struct ScheduleData
		{
			internal Task? CurrentTask;
			internal Queue<IClientTask>? QueuedTasks;
		}
	}
}
