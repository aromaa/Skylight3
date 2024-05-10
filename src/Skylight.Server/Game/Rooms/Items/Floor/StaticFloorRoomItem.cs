using Skylight.API.Game.Furniture.Floor;
using Skylight.API.Game.Rooms;
using Skylight.API.Game.Rooms.Items.Floor;
using Skylight.API.Game.Users;
using Skylight.API.Numerics;

namespace Skylight.Server.Game.Rooms.Items.Floor;

internal sealed class StaticFloorRoomItem : FloorRoomItem, IStaticFloorRoomItem
{
	public override IStaticFloorFurniture Furniture { get; }

	internal StaticFloorRoomItem(IRoom room, int id, IUserInfo owner, IStaticFloorFurniture furniture, Point3D position, int direction)
		: base(room, id, owner, position, direction)
	{
		this.Furniture = furniture;
	}

	public override double Height => this.Furniture.DefaultHeight;
}
