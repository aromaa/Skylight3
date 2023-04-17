using System.Runtime.CompilerServices;
using Skylight.API.Game.Rooms;

namespace Skylight.Server.Game.Rooms.Scheduler.Tasks;

internal sealed class AsyncFuncRoomTask<TState> : IRoomTask
{
	private readonly TaskCompletionSource taskCompletionSource;

	private readonly Func<IRoom, TState, ValueTask> func;
	private readonly TState state;

	internal AsyncFuncRoomTask(Func<IRoom, TState, ValueTask> func, TState state)
	{
		this.taskCompletionSource = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

		this.func = func;
		this.state = state;
	}

	internal Task Task => this.taskCompletionSource.Task;

	public void Execute(IRoom room)
	{
		try
		{
			ValueTask task = this.func(room, this.state);
			if (task.IsCompletedSuccessfully)
			{
				this.taskCompletionSource.SetResult();

				return;
			}

			task.AsTask().ContinueWith(static (task, state) =>
			{
				if (task.IsCompletedSuccessfully)
				{
					Unsafe.As<TaskCompletionSource>(state!).SetResult();
				}
				else if (task.IsFaulted)
				{
					Unsafe.As<TaskCompletionSource>(state!).SetException(task.Exception);
				}
				else
				{
					Unsafe.As<TaskCompletionSource>(state!).SetCanceled();
				}
			}, this.taskCompletionSource, TaskContinuationOptions.ExecuteSynchronously);
		}
		catch (Exception e)
		{
			this.taskCompletionSource.SetException(e);
		}
	}

	internal void Cancel() => this.taskCompletionSource.SetCanceled();
}
