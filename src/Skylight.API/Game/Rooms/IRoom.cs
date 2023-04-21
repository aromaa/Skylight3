using Skylight.API.Game.Rooms.Items;
using Skylight.API.Game.Rooms.Map;
using Skylight.API.Game.Rooms.Units;
using Skylight.API.Game.Users;
using Skylight.Protocol.Packets.Outgoing;

namespace Skylight.API.Game.Rooms;

public interface IRoom
{
	public IRoomInfo Info { get; }

	public IRoomMap Map { get; }

	public IRoomItemManager ItemManager { get; }
	public IRoomUnitManager UnitManager { get; }

	public int GameTime { get; }

	ValueTask SendAsync<T>(in T packet)
		where T : IGameOutgoingPacket;

	public Task LoadAsync(CancellationToken cancellationToken = default);

	public void Enter(IUser user);
	public void Exit(IUser user);

	public bool ScheduleTask<TTask>(in TTask task)
		where TTask : IRoomTask;

	public bool ScheduleTask(Action<IRoom> action);
	public bool ScheduleTask<TState>(Action<IRoom, TState> action, in TState state);

	public ValueTask ScheduleTaskAsync<TTask>(in TTask task)
		where TTask : IRoomTask;

	public ValueTask ScheduleTaskAsync(Action<IRoom> action);
	public ValueTask ScheduleTaskAsync<TState>(Action<IRoom, TState> action, in TState state);

	public ValueTask ScheduleTaskAsync(Func<IRoom, ValueTask> func);
	public ValueTask ScheduleTaskAsync<TState>(Func<IRoom, TState, ValueTask> func, in TState state);

	public ValueTask<TOut> ScheduleTaskAsync<TOut>(Func<IRoom, TOut> func);
	public ValueTask<TOut> ScheduleTaskAsync<TState, TOut>(Func<IRoom, TState, TOut> func, in TState state);

	public ValueTask<TOut> ScheduleTaskAsync<TOut>(Func<IRoom, ValueTask<TOut>> func);
	public ValueTask<TOut> ScheduleTaskAsync<TState, TOut>(Func<IRoom, TState, ValueTask<TOut>> func, in TState state);

	public void ScheduleUpdateTask(IRoomTask task);
}
