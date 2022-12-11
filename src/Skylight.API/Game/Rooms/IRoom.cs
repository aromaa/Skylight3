using Skylight.API.Game.Rooms.Items;
using Skylight.API.Game.Rooms.Map;
using Skylight.API.Game.Rooms.Units;
using Skylight.API.Game.Users;

namespace Skylight.API.Game.Rooms;

public interface IRoom
{
	public IRoomInfo Info { get; }

	public IRoomMap Map { get; }

	public IRoomItemManager ItemManager { get; }
	public IRoomUnitManager UnitManager { get; }

	public Task LoadAsync(CancellationToken cancellationToken = default);

	public void Enter(IUser user);
	public void Exit(IUser user);

	public void ScheduleTask<T>(in T task)
		where T : IRoomTask;
	public ValueTask<TOut> ScheduleTaskAsync<TTask, TOut>(in TTask task)
		where TTask : IRoomTask<TOut>;
}
