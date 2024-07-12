using Skylight.API.Game.Furniture.Floor;
using Skylight.API.Game.Rooms.Items.Floor;
using Skylight.API.Game.Rooms.Private;
using Skylight.API.Game.Users;
using Skylight.API.Numerics;

namespace Skylight.Server.Game.Rooms.Items.Floor;

internal sealed class StaticFloorRoomItem(IPrivateRoom room, int id, IUserInfo owner, IStaticFloorFurniture furniture, Point3D position, int direction)
	: PlainFloorRoomItem<IStaticFloorFurniture>(room, id, owner, furniture, position, direction), IStaticFloorRoomItem
{
	public new IStaticFloorFurniture Furniture => this.furniture;
}
