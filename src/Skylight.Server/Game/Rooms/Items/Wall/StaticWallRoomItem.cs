using Skylight.API.Game.Furniture.Wall;
using Skylight.API.Game.Rooms;
using Skylight.API.Game.Rooms.Items.Wall;
using Skylight.API.Game.Users;
using Skylight.API.Numerics;

namespace Skylight.Server.Game.Rooms.Items.Wall;

internal sealed class StaticWallRoomItem : WallRoomItem, IStaticWallRoomItem
{
	public override IStaticWallFurniture Furniture { get; }

	public int State { get; set; }

	internal StaticWallRoomItem(IRoom room, int id, IUserInfo owner, IStaticWallFurniture furniture, Point2D location, Point2D position, int direction)
		: base(room, id, owner, location, position, direction)
	{
		this.Furniture = furniture;
	}
}
