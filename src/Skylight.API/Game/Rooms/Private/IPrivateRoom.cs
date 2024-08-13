using Skylight.API.Game.Rooms.Items;
using Skylight.API.Game.Rooms.Map;
using Skylight.API.Game.Rooms.Map.Private;
using Skylight.API.Game.Users;

namespace Skylight.API.Game.Rooms.Private;

public interface IPrivateRoom : IRoom
{
	public new IPrivateRoomInfo Info { get; }
	public new IPrivateRoomMap Map { get; }

	public IRoomItemManager ItemManager { get; }

	public bool IsOwner(IUser user);

	IRoomInfo IRoom.Info => this.Info;
	IRoomMap IRoom.Map => this.Map;
}
