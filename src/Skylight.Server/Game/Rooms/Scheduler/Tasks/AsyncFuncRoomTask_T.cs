using System.Runtime.CompilerServices;
using Skylight.API.Game.Rooms;

namespace Skylight.Server.Game.Rooms.Scheduler.Tasks;

internal sealed class AsyncFuncRoomTask<TState, TOut> : IRoomTask
{
	private readonly TaskCompletionSource<TOut> taskCompletionSource;

	private readonly Func<IRoom, TState, ValueTask<TOut>> func;
	private readonly TState state;

	internal AsyncFuncRoomTask(Func<IRoom, TState, ValueTask<TOut>> func, TState state)
	{
		this.taskCompletionSource = new TaskCompletionSource<TOut>(TaskCreationOptions.RunContinuationsAsynchronously);

		this.func = func;
		this.state = state;
	}

	internal Task<TOut> Task => this.taskCompletionSource.Task;

	public void Execute(IRoom room)
	{
		try
		{
			ValueTask<TOut> task = this.func(room, this.state);
			if (task.IsCompletedSuccessfully)
			{
				this.taskCompletionSource.SetResult(task.Result);

				return;
			}

			task.AsTask().ContinueWith(static (task, state) =>
			{
				if (task.IsCompletedSuccessfully)
				{
					Unsafe.As<TaskCompletionSource<TOut>>(state!).SetResult(task.Result);
				}
				else if (task.IsFaulted)
				{
					Unsafe.As<TaskCompletionSource<TOut>>(state!).SetException(task.Exception);
				}
				else
				{
					Unsafe.As<TaskCompletionSource<TOut>>(state!).SetCanceled();
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
