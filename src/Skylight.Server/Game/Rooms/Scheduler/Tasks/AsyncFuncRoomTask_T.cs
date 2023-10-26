using Skylight.API.Game.Rooms;
using Skylight.Server.Extensions;

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

			this.taskCompletionSource.SetFromTask(task.AsTask());
		}
		catch (Exception e)
		{
			this.taskCompletionSource.SetException(e);
		}
	}

	internal void Cancel() => this.taskCompletionSource.SetCanceled();
}
