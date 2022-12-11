using System.Text.Json;
using Skylight.API.Game.Furniture.Floor;
using Skylight.API.Game.Rooms;
using Skylight.API.Game.Users;
using Skylight.API.Numerics;

namespace Skylight.Server.Game.Rooms.Items.Floor.Factory;

internal sealed class BasicFloorRoomItemFactory : FloorRoomItemFactory<IBasicFloorFurniture, BasicFloorRoomItem>
{
	public override BasicFloorRoomItem Create(IRoom room, int itemId, IUserInfo owner, IBasicFloorFurniture furniture, Point3D position, int direction, JsonDocument? extraData)
	{
		return new BasicFloorRoomItem(room, itemId, owner, furniture, position, direction);
	}
}
