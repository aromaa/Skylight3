using Skylight.API.Game.Rooms;

namespace Skylight.Server.Game.Rooms.Scheduler.Tasks;

internal sealed class ActionRoomTask<TState> : IRoomTask
{
	private readonly Action<IRoom, TState> action;
	private readonly TState state;

	internal ActionRoomTask(Action<IRoom, TState> action, TState state)
	{
		this.action = action;
		this.state = state;
	}

	public void Execute(IRoom room) => this.action(room, this.state);
}
