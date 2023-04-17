using Skylight.API.Game.Rooms;

namespace Skylight.Server.Game.Rooms.Scheduler.Tasks;

internal sealed class FuncRoomTask<TState> : IRoomTask
{
	private readonly Func<IRoom, TState, ValueTask> func;
	private readonly TState state;

	internal FuncRoomTask(Func<IRoom, TState, ValueTask> func, TState state)
	{
		this.func = func;
		this.state = state;
	}

	public void Execute(IRoom room) => this.func(room, this.state);
}
