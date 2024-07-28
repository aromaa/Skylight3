using Skylight.API.Game.Rooms.Map;
using Skylight.API.Game.Rooms.Units;
using Skylight.API.Game.Users;
using Skylight.Protocol.Packets.Outgoing;

namespace Skylight.API.Game.Rooms;

public interface IRoom
{
	public IRoomInfo Info { get; }

	public IRoomMap Map { get; }

	public IRoomUnitManager UnitManager { get; }

	public int GameTime { get; }

	public void SendAsync<T>(in T packet)
		where T : IGameOutgoingPacket;

	public Task LoadAsync(CancellationToken cancellationToken = default);

	public void Enter(IUser user);
	public void Exit(IUser user);

	public bool PostTask<TTask>(TTask task)
		where TTask : IRoomTask;

	public ValueTask PostTaskAsync<TTask>(TTask task)
		where TTask : IRoomTask;

	public ValueTask<TResult> ScheduleTask<TTask, TResult>(TTask task)
		where TTask : IRoomTask<TResult>;

	public ValueTask<TResult> ScheduleTaskAsync<TTask, TResult>(TTask task)
		where TTask : IAsyncRoomTask<TResult>;

	public bool PostTask(Action<IRoom> action);
	public ValueTask PostTaskAsync(Action<IRoom> action);
	public ValueTask<TResult> ScheduleTask<TResult>(Func<IRoom, TResult> func);
	public ValueTask<TResult> ScheduleTaskAsync<TResult>(Func<IRoom, ValueTask<TResult>> func);

	public void ScheduleUpdateTask(IRoomTask task);

	public void Unload();
}
