using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Channels;
using CommunityToolkit.HighPerformance;
using Microsoft.EntityFrameworkCore;
using Net.Collections;
using Skylight.API.Game.Furniture;
using Skylight.API.Game.Rooms;
using Skylight.API.Game.Rooms.Items;
using Skylight.API.Game.Rooms.Items.Floor;
using Skylight.API.Game.Rooms.Items.Interactions;
using Skylight.API.Game.Rooms.Items.Wall;
using Skylight.API.Game.Rooms.Map;
using Skylight.API.Game.Rooms.Units;
using Skylight.API.Game.Users;
using Skylight.Infrastructure;
using Skylight.Protocol.Packets.Outgoing;
using Skylight.Server.Extensions;
using Skylight.Server.Game.Rooms.GameMap;
using Skylight.Server.Game.Rooms.Items;
using Skylight.Server.Game.Rooms.Units;

namespace Skylight.Server.Game.Rooms;

internal sealed class Room : IRoom
{
	public IRoomInfo Info { get; }

	public IRoomMap Map { get; }
	public IRoomUnitManager UnitManager { get; }
	public IRoomItemManager ItemManager { get; }

	private SpinLock tickingLock; //Note: Mutating struct
	private SpinLock scheduledTasksLock; //Note: mutating struct

	private readonly Channel<IRoomTask> scheduledTasks;

	private readonly SocketCollection roomClients;

	private readonly Thread thread;

	public Room(RoomData roomData, IDbContextFactory<SkylightContext> dbContextFactory, IFurnitureManager furnitureManager, IFloorRoomItemStrategy floorRoomItemStrategy, IWallRoomItemStrategy wallRoomItemStrategy, IRoomItemInteractionManager itemInteractionManager, IUserManager userManager)
	{
		this.Info = roomData;

		this.Map = new RoomTileMap(this, roomData.Layout);
		this.UnitManager = new RoomUnitManager(this);
		this.ItemManager = new RoomItemManager(this, dbContextFactory, furnitureManager, floorRoomItemStrategy, wallRoomItemStrategy, itemInteractionManager, userManager);

		this.tickingLock = new SpinLock(enableThreadOwnerTracking: false);
		this.scheduledTasksLock = new SpinLock(enableThreadOwnerTracking: false);

		this.scheduledTasks = Channel.CreateUnbounded<IRoomTask>(new UnboundedChannelOptions
		{
			SingleReader = true
		});

		this.roomClients = new SocketCollection();

		this.thread = new Thread(this.DoTicking)
		{
			IsBackground = true
		};
		this.thread.Start();
	}

	internal int UserCount => this.roomClients.Count;

	public async Task LoadAsync(CancellationToken cancellationToken)
	{
		await this.ItemManager.LoadAsync(cancellationToken).ConfigureAwait(false);
	}

	public void Enter(IUser user)
	{
		this.roomClients.TryAdd(user.Client.Socket);
	}

	public void Exit(IUser user)
	{
		this.roomClients.TryRemove(user.Client.Socket);
	}

	private void DoTicking()
	{
		const int TickTime = 500;

		while (true)
		{
			long startTime = Stopwatch.GetTimestamp();

			this.Tick();

			Thread.Sleep(Math.Max(TickTime, TickTime - Stopwatch.GetElapsedTime(startTime).Milliseconds));
		}
	}

	internal void Tick()
	{
		//Tick room inside the lock!
		using (this.tickingLock.Enter())
		{
			this.UnitManager.Tick();

			//After everything is done, run the tasks we received while ticking
			using (this.scheduledTasksLock.Enter())
			{
				this.ExecuteTasksNoLock();
			}
		}
	}

	private void ExecuteTasksNoLock()
	{
		while (this.scheduledTasks.Reader.TryRead(out IRoomTask? task))
		{
			task.Execute(this);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void ScheduleTask<T>(in T task)
		where T : IRoomTask
	{
		//If we can grab the ticking lock, we can just execute
		//the task without needing to worry about threading
		if (this.tickingLock.TryEnter())
		{
			try
			{
				task.Execute(this);
			}
			finally
			{
				this.tickingLock.Exit();
			}
		}
		else
		{
			this.ScheduleTaskSlow(task);
		}
	}

	public ValueTask<TOut> ScheduleTaskAsync<TTask, TOut>(in TTask task)
		where TTask : IRoomTask<TOut>
	{
		//If we can grab the ticking lock, we can just execute
		//the task without needing to worry about threading
		if (this.tickingLock.TryEnter())
		{
			try
			{
				return ValueTask.FromResult(task.Execute(this));
			}
			finally
			{
				this.tickingLock.Exit();
			}
		}
		else
		{
			AsyncRoomTask<TOut> asyncTask = new(task);

			this.ScheduleTaskSlow(asyncTask);

			return new ValueTask<TOut>(asyncTask.Task);
		}
	}

	private void ScheduleTaskSlow(IRoomTask task)
	{
		//Schedule the task to be run after ticking lock releases
		if (!this.scheduledTasks.Writer.TryWrite(task))
		{
			//We are disposing, nothing to run anymore
			return;
		}

		//Are we currently executing the tasks?
		if (this.scheduledTasksLock.TryEnter())
		{
			try
			{
				//We are not currently executing tasks, but are we still ticking?
				if (this.tickingLock.TryEnter())
				{
					//We aren't ticking, try to run all of the schedules tasks
					try
					{
						this.ExecuteTasksNoLock();
					}
					finally
					{
						this.tickingLock.Exit();
					}
				}
				else
				{
					//We are still ticking, the scheduled tasks will be run as soon as its done
				}
			}
			finally
			{
				this.scheduledTasksLock.Exit();
			}
		}
		else
		{
			//We are currently executing tasks, try to enter to the lock
			//to ensure we run the task asap as we might already be
			//exiting the loop while posted the task
			using (this.tickingLock.Enter())
			{
				using (this.scheduledTasksLock.Enter())
				{
					this.ExecuteTasksNoLock();
				}
			}
		}
	}

	internal ValueTask SendAsync<T>(in T packet)
		where T : IGameOutgoingPacket
	{
		return new ValueTask(this.roomClients.SendAsync(packet));
	}

	private sealed class AsyncRoomTask<T> : IRoomTask
	{
		private readonly TaskCompletionSource<T> taskCompletionSource;

		private readonly IRoomTask<T> task;

		internal AsyncRoomTask(IRoomTask<T> task)
		{
			this.taskCompletionSource = new TaskCompletionSource<T>();

			this.task = task;
		}

		internal Task<T> Task => this.taskCompletionSource.Task;

		public void Execute(IRoom room)
		{
			this.taskCompletionSource.SetResult(this.task.Execute(room));
		}
	}
}
