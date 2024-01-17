using Skylight.API.Game.Rooms;
using Skylight.Server.Extensions;

namespace Skylight.Server.Game.Rooms.Scheduler.Tasks;

internal sealed class AsyncResultRoomTask<TTask, TResult> : IRoomTask
	where TTask : IAsyncRoomTask<TResult>
{
	private readonly TaskCompletionSource<TResult> taskCompletionSource;

	private readonly TTask task;

	internal AsyncResultRoomTask(TTask task)
	{
		this.taskCompletionSource = new TaskCompletionSource<TResult>(TaskCreationOptions.RunContinuationsAsynchronously);

		this.task = task;
	}

	internal Task<TResult> Task => this.taskCompletionSource.Task;

	public void Execute(IRoom room)
	{
		try
		{
			ValueTask<TResult> task = this.task.Execute(room);
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
