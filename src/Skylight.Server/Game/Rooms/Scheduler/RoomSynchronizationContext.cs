namespace Skylight.Server.Game.Rooms.Scheduler;

internal sealed class RoomSynchronizationContext : SynchronizationContext
{
	private readonly RoomTaskScheduler roomScheduler;

	internal RoomSynchronizationContext(RoomTaskScheduler roomScheduler)
	{
		this.roomScheduler = roomScheduler;
	}

	public override void Post(SendOrPostCallback callback, object? state) => this.roomScheduler.ScheduleTask(static (_, state) => state.callback(state), (callback, state));
	public override void Send(SendOrPostCallback callback, object? state) => this.roomScheduler.ScheduleTaskAsync(static (_, state) => state.callback(state), (callback, state)).GetAwaiter().GetResult();

	public override RoomSynchronizationContext CreateCopy() => new(this.roomScheduler);
}
