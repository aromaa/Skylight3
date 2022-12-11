using Skylight.API.Game.Furniture.Floor;
using Skylight.API.Game.Rooms;
using Skylight.API.Game.Rooms.Items.Floor;
using Skylight.API.Game.Users;
using Skylight.API.Numerics;

namespace Skylight.Server.Game.Rooms.Items.Floor;

internal sealed class BasicFloorRoomItem : FloorRoomItem, IBasicFloorRoomItem
{
	public override IBasicFloorFurniture Furniture { get; }

	internal BasicFloorRoomItem(IRoom room, int id, IUserInfo owner, IBasicFloorFurniture furniture, Point3D position, int direction)
		: base(room, id, owner, position, direction)
	{
		this.Furniture = furniture;
	}
}
