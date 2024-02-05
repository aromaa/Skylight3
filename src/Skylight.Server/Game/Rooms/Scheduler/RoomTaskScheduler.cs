using System.Threading.Channels;
using CommunityToolkit.HighPerformance;
using Skylight.API.Game.Rooms;
using Skylight.Server.Extensions;
using Skylight.Server.Game.Rooms.Scheduler.Tasks;

namespace Skylight.Server.Game.Rooms.Scheduler;

internal sealed class RoomTaskScheduler
{
	private readonly Room room;

	private readonly Channel<IRoomTask> scheduledTasks;

	private SpinLock scheduledTasksLock; //Note: mutating struct

	private readonly RoomSynchronizationContext synchronizationContext;

	internal RoomTaskScheduler(Room room)
	{
		this.room = room;

		this.scheduledTasks = Channel.CreateUnbounded<IRoomTask>(new UnboundedChannelOptions
		{
			SingleReader = true
		});

		this.scheduledTasksLock = new SpinLock(enableThreadOwnerTracking: false);

		this.synchronizationContext = new RoomSynchronizationContext(this);
	}

	public bool PostTask<TTask>(TTask task)
		where TTask : IRoomTask => this.ScheduleTaskInternal<RawRoomTaskScheduler<TTask>, bool>(new RawRoomTaskScheduler<TTask>(task));

	public ValueTask PostTaskAsync<TTask>(TTask task)
		where TTask : IRoomTask => this.ScheduleTaskInternal<AsyncRoomTaskScheduler<TTask>, ValueTask>(new AsyncRoomTaskScheduler<TTask>(task));

	public ValueTask<TResult> ScheduleTask<TTask, TResult>(in TTask task)
		where TTask : IRoomTask<TResult> => this.ScheduleTaskInternal<ResultRoomTaskScheduler<TTask, TResult>, ValueTask<TResult>>(new ResultRoomTaskScheduler<TTask, TResult>(task));
	public ValueTask<TResult> ScheduleTaskAsync<TTask, TResult>(in TTask task)
		where TTask : IAsyncRoomTask<TResult> => this.ScheduleTaskInternal<AsyncResultRoomTaskScheduler<TTask, TResult>, ValueTask<TResult>>(new AsyncResultRoomTaskScheduler<TTask, TResult>(task));

	public bool PostTask(Action<IRoom> action)
	{
#if !DEBUG
		throw new NotSupportedException();
#else
		return this.PostTask(new ActionRoomTaskWrapped(action));
#endif
	}

	public ValueTask PostTaskAsync(Action<IRoom> action)
	{
#if !DEBUG
		throw new NotSupportedException();
#else
		return this.PostTaskAsync(new ActionRoomTaskWrapped(action));
#endif
	}

	public ValueTask<TResult> ScheduleTask<TResult>(Func<IRoom, TResult> func)
	{
#if !DEBUG
		throw new NotSupportedException();
#else
		return this.ScheduleTask<FuncRoomTaskWrapped<TResult>, TResult>(new FuncRoomTaskWrapped<TResult>(func));
#endif
	}

	public ValueTask<TResult> ScheduleTaskAsync<TResult>(Func<IRoom, ValueTask<TResult>> func)
	{
#if !DEBUG
		throw new NotSupportedException();
#else
		return this.ScheduleTaskAsync<AsyncFuncRoomTaskWrapped<TResult>, TResult>(new AsyncFuncRoomTaskWrapped<TResult>(func));
#endif
	}

	private TResult ScheduleTaskInternal<TTask, TResult>(in TTask action)
		where TTask : IRoomTaskScheduler<TResult>
	{
		if (this.room.TickingLock.TryEnter())
		{
			SynchronizationContext? context = SynchronizationContext.Current;

			try
			{
				SynchronizationContext.SetSynchronizationContext(this.synchronizationContext);

				return action.Execute(this.room);
			}
			catch (Exception exception)
			{
				//Cancel room

				return action.HandleException(exception);
			}
			finally
			{
				SynchronizationContext.SetSynchronizationContext(context);

				this.room.TickingLock.Exit();
			}
		}
		else
		{
			return action.CreateTask(this);
		}
	}

	private bool ScheduleTaskSlow(IRoomTask task)
	{
		//Schedule the task to be run after ticking lock releases
		if (!this.scheduledTasks.Writer.TryWrite(task))
		{
			//We are disposing, nothing to run anymore
			return false;
		}

		//Are we currently executing the tasks?
		if (this.scheduledTasksLock.TryEnter())
		{
			try
			{
				//We are not currently executing tasks, but are we still ticking?
				if (this.room.TickingLock.TryEnter())
				{
					//We aren't ticking, try to run all of the schedules tasks
					try
					{
						this.ExecuteTasksNoLock();
					}
					finally
					{
						this.room.TickingLock.Exit();
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
			using (this.room.TickingLock.Enter())
			{
				using (this.scheduledTasksLock.Enter())
				{
					this.ExecuteTasksNoLock();
				}
			}
		}

		return true;
	}

	private void ExecuteTasksNoLock()
	{
		SynchronizationContext? context = SynchronizationContext.Current;

		try
		{
			SynchronizationContext.SetSynchronizationContext(this.synchronizationContext);

			while (this.scheduledTasks.Reader.TryRead(out IRoomTask? task))
			{
				task.Execute(this.room);
			}
		}
		finally
		{
			SynchronizationContext.SetSynchronizationContext(context);
		}
	}

	internal void ExecuteTasks()
	{
		using (this.scheduledTasksLock.Enter())
		{
			this.ExecuteTasksNoLock();
		}
	}

	private readonly struct RawRoomTaskScheduler<TTask>(TTask task) : IRoomTaskScheduler<bool>
		where TTask : IRoomTask
	{
		public bool Execute(Room room)
		{
			task.Execute(room);

			return true;
		}

		public bool CreateTask(RoomTaskScheduler scheduler) => scheduler.ScheduleTaskSlow(task);

		public bool HandleException(Exception exception) => false;
	}

	private readonly struct AsyncRoomTaskScheduler<TTask>(TTask task) : IRoomTaskScheduler<ValueTask>
		where TTask : IRoomTask
	{
		public ValueTask Execute(Room room)
		{
			task.Execute(room);

			return ValueTask.CompletedTask;
		}

		public ValueTask CreateTask(RoomTaskScheduler scheduler)
		{
			AsyncRoomTask<TTask> asyncTask = new(task);

			scheduler.ScheduleTaskSlow(asyncTask);

			return new ValueTask(asyncTask.Task);
		}

		public ValueTask HandleException(Exception exception) => ValueTask.FromException(exception);
	}

	private readonly struct ResultRoomTaskScheduler<TTask, TResult>(TTask task) : IRoomTaskScheduler<ValueTask<TResult>>
		where TTask : IRoomTask<TResult>
	{
		public ValueTask<TResult> Execute(Room room) => ValueTask.FromResult(task.Execute(room));

		public ValueTask<TResult> CreateTask(RoomTaskScheduler scheduler)
		{
			ResultRoomTask<TTask, TResult> asyncTask = new(task);

			scheduler.ScheduleTaskSlow(asyncTask);

			return new ValueTask<TResult>(asyncTask.Task);
		}

		public ValueTask<TResult> HandleException(Exception exception) => ValueTask.FromException<TResult>(exception);
	}

	private readonly struct AsyncResultRoomTaskScheduler<TTask, TResult>(TTask task) : IRoomTaskScheduler<ValueTask<TResult>>
		where TTask : IAsyncRoomTask<TResult>
	{
		public ValueTask<TResult> Execute(Room room) => task.Execute(room);

		public ValueTask<TResult> CreateTask(RoomTaskScheduler scheduler)
		{
			AsyncResultRoomTask<TTask, TResult> asyncTask = new(task);

			scheduler.ScheduleTaskSlow(asyncTask);

			return new ValueTask<TResult>(asyncTask.Task);
		}

		public ValueTask<TResult> HandleException(Exception exception) => ValueTask.FromException<TResult>(exception);
	}

	private readonly struct ActionRoomTaskWrapped(Action<IRoom> action) : IRoomTask
	{
		public void Execute(IRoom room) => action(room);
	}

	private readonly struct FuncRoomTaskWrapped<T>(Func<IRoom, T> action) : IRoomTask<T>
	{
		public T Execute(IRoom room) => action(room);
	}

	private readonly struct AsyncFuncRoomTaskWrapped<T>(Func<IRoom, ValueTask<T>> action) : IAsyncRoomTask<T>
	{
		public ValueTask<T> Execute(IRoom room) => action(room);
	}
}
