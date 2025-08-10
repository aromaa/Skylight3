using Skylight.API.Game.Rooms.Private;

namespace Skylight.API.Game.Inventory;

public interface INavigatorSearch
{
	public IEnumerable<IPrivateRoomInfo> PopularRooms { get; }
}
