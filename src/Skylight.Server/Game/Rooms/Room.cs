using System.Diagnostics;
using CommunityToolkit.HighPerformance;
using Net.Collections;
using Skylight.API.Game.Rooms;
using Skylight.API.Game.Rooms.Map;
using Skylight.API.Game.Rooms.Units;
using Skylight.API.Game.Users;
using Skylight.Protocol.Packets.Outgoing;
using Skylight.Server.Game.Rooms.Scheduler;

namespace Skylight.Server.Game.Rooms;

internal abstract class Room : IRoom
{
	public IRoomInfo Info { get; }

	public abstract IRoomMap Map { get; }
	public abstract IRoomUnitManager UnitManager { get; }

	internal RoomTaskScheduler RoomTaskScheduler { get; }

	private SpinLock tickingLock; //Note: Mutating struct

	private readonly Queue<IRoomTask> scheduledUpdateTasks;

	private readonly SocketCollection roomClients;

	private readonly Thread thread;

	private bool active = true;

	protected Room(RoomData roomData, IRoomLayout roomLayout)
	{
		this.Info = roomData;

		this.tickingLock = new SpinLock(enableThreadOwnerTracking: false);

		this.RoomTaskScheduler = new RoomTaskScheduler(this);

		this.scheduledUpdateTasks = new Queue<IRoomTask>();

		this.roomClients = new SocketCollection();

		this.thread = new Thread(this.DoTicking)
		{
			IsBackground = true
		};
	}

	internal ref SpinLock TickingLock => ref this.tickingLock;

	public int GameTime => 0;

	internal int UserCount => this.roomClients.Count;

	public abstract Task LoadAsync(CancellationToken cancellationToken);

	public void Start()
	{
		this.thread.Start();
	}

	public void Enter(IUser user)
	{
		this.roomClients.TryAdd(user.Client.Socket);
	}

	public void Exit(IUser user)
	{
		this.roomClients.TryRemove(user.Client.Socket);
	}

	public bool IsOwner(IUser user) => this.Info.Owner.Id == user.Profile.Id;

	private void DoTicking()
	{
		const int TickTime = 500;

		while (this.active)
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
			IRoomTask[] tasks;
			lock (this.scheduledUpdateTasks)
			{
				tasks = [.. this.scheduledUpdateTasks];

				this.scheduledUpdateTasks.Clear();
			}

			foreach (IRoomTask roomTask in tasks)
			{
				roomTask.Execute(this);
			}

			this.UnitManager.Tick();

			this.DoTick();

			//After everything is done, run the tasks we received while ticking
			this.RoomTaskScheduler.ExecuteTasks();
		}
	}

	internal abstract void DoTick();

	public bool PostTask<TTask>(TTask task)
		where TTask : IRoomTask => this.RoomTaskScheduler.PostTask(task);

	public ValueTask PostTaskAsync<TTask>(TTask task)
		where TTask : IRoomTask => this.RoomTaskScheduler.PostTaskAsync(task);

	public ValueTask<TResult> ScheduleTask<TTask, TResult>(TTask task)
		where TTask : IRoomTask<TResult> => this.RoomTaskScheduler.ScheduleTask<TTask, TResult>(task);

	public ValueTask<TResult> ScheduleTaskAsync<TTask, TResult>(TTask task)
		where TTask : IAsyncRoomTask<TResult> => this.RoomTaskScheduler.ScheduleTaskAsync<TTask, TResult>(task);

	public bool PostTask(Action<IRoom> action) => this.RoomTaskScheduler.PostTask(action);
	public ValueTask PostTaskAsync(Action<IRoom> action) => this.RoomTaskScheduler.PostTaskAsync(action);
	public ValueTask<TResult> ScheduleTask<TResult>(Func<IRoom, TResult> func) => this.RoomTaskScheduler.ScheduleTask(func);
	public ValueTask<TResult> ScheduleTaskAsync<TResult>(Func<IRoom, ValueTask<TResult>> func) => this.RoomTaskScheduler.ScheduleTaskAsync(func);

	public void ScheduleUpdateTask(IRoomTask task)
	{
		lock (this.scheduledUpdateTasks)
		{
			this.scheduledUpdateTasks.Enqueue(task);
		}
	}

	public void SendAsync<T>(in T packet)
		where T : IGameOutgoingPacket
	{
		this.roomClients.SendAsync(packet);
	}

	public void Unload()
	{
		this.active = false;
	}
}
