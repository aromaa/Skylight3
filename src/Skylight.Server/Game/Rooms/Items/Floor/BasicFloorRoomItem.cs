using Skylight.API.Game.Furniture.Floor;
using Skylight.API.Game.Rooms;
using Skylight.API.Game.Rooms.Items.Floor;
using Skylight.API.Game.Users;
using Skylight.API.Numerics;

namespace Skylight.Server.Game.Rooms.Items.Floor;

internal sealed class BasicFloorRoomItem(IRoom room, int id, IUserInfo owner, IBasicFloorFurniture furniture, Point3D position, int direction, int state)
	: MultiStateFloorRoomItem<IBasicFloorFurniture>(room, id, owner, furniture, position, direction, state), IBasicFloorRoomItem
{
	public new IBasicFloorFurniture Furniture => this.furniture;
}
