using Skylight.API.Game.Rooms.Map;
using Skylight.API.Game.Rooms.Map.Public;

namespace Skylight.API.Game.Rooms.Public;

public interface IPublicRoom : IRoom
{
	public new IPublicRoomMap Map { get; }

	IRoomMap IRoom.Map => this.Map;
}
