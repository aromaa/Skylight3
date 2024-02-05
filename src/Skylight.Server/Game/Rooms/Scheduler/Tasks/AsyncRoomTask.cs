using Skylight.API.Game.Rooms;

namespace Skylight.Server.Game.Rooms.Scheduler.Tasks;

internal sealed class AsyncRoomTask<TTask> : IRoomTask
	where TTask : IRoomTask
{
	private readonly TaskCompletionSource taskCompletionSource;

	private readonly TTask task;

	internal AsyncRoomTask(TTask task)
	{
		this.taskCompletionSource = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

		this.task = task;
	}

	internal Task Task => this.taskCompletionSource.Task;

	public void Execute(IRoom room)
	{
		try
		{
			this.task.Execute(room);

			this.taskCompletionSource.SetResult();
		}
		catch (Exception e)
		{
			this.taskCompletionSource.SetException(e);

			throw;
		}
	}

	internal void Cancel() => this.taskCompletionSource.SetCanceled();
}
