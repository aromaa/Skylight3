using Skylight.API.Game.Rooms;

namespace Skylight.Server.Game.Rooms.Scheduler;

internal sealed class RoomSynchronizationContext : SynchronizationContext
{
	private readonly RoomTaskScheduler roomScheduler;

	internal RoomSynchronizationContext(RoomTaskScheduler roomScheduler)
	{
		this.roomScheduler = roomScheduler;
	}

	public override void Post(SendOrPostCallback callback, object? state) => this.roomScheduler.PostTask(new WrappedSendOrPostCallback(callback, state));
	public override void Send(SendOrPostCallback callback, object? state) => this.roomScheduler.PostTaskAsync(new WrappedSendOrPostCallback(callback, state)).GetAwaiter().GetResult();

	public override RoomSynchronizationContext CreateCopy() => new(this.roomScheduler);

	private readonly struct WrappedSendOrPostCallback(SendOrPostCallback callback, object? state) : IRoomTask
	{
		public void Execute(IRoom room) => callback(state);
	}
}
