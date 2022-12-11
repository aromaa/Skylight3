using System.Text.Json;
using Skylight.API.Game.Furniture.Wall;
using Skylight.API.Game.Rooms;
using Skylight.API.Game.Users;
using Skylight.API.Numerics;

namespace Skylight.Server.Game.Rooms.Items.Wall.Factory;

internal sealed class BasicWallRoomItemFactory : WallRoomItemFactory<IBasicWallFurniture, BasicWallRoomItem>
{
	public override BasicWallRoomItem Create(IRoom room, int itemId, IUserInfo owner, IBasicWallFurniture furniture, Point2D location, Point2D position, JsonDocument? extraData)
	{
		return new BasicWallRoomItem(room, itemId, owner, furniture, location, position, 0);
	}
}
