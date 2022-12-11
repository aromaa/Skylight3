using Skylight.API.Game.Users;

namespace Skylight.API.Game.Rooms.Units;

public interface IRoomUnitManager
{
	public IEnumerable<IRoomUnit> Units { get; }

	//TODO: Factory?
	public IUserRoomUnit CreateUnit(IUser user);

	public void AddUnit(IRoomUnit unit);

	public void RemoveUnit(IRoomUnit unit);

	public void Tick();
}
