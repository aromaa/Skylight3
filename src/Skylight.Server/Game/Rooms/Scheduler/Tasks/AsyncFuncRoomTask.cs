using Skylight.API.Game.Rooms;
using Skylight.Server.Extensions;

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

			this.taskCompletionSource.SetFromTask(task.AsTask());
		}
		catch (Exception e)
		{
			this.taskCompletionSource.SetException(e);
		}
	}

	internal void Cancel() => this.taskCompletionSource.SetCanceled();
}
