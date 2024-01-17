using Skylight.API.Game.Rooms;

namespace Skylight.Server.Game.Rooms.Scheduler.Tasks;

internal sealed class ResultRoomTask<TTask, TResult> : IRoomTask
	where TTask : IRoomTask<TResult>
{
	private readonly TaskCompletionSource<TResult> taskCompletionSource;

	private readonly TTask task;

	internal ResultRoomTask(TTask task)
	{
		this.taskCompletionSource = new TaskCompletionSource<TResult>(TaskCreationOptions.RunContinuationsAsynchronously);

		this.task = task;
	}

	internal Task<TResult> Task => this.taskCompletionSource.Task;

	public void Execute(IRoom room)
	{
		try
		{
			this.taskCompletionSource.SetResult(this.task.Execute(room));
		}
		catch (Exception e)
		{
			this.taskCompletionSource.SetException(e);
		}
	}

	internal void Cancel() => this.taskCompletionSource.SetCanceled();
}
