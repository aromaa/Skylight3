using System.Text.Json;
using Skylight.API.Game.Furniture.Floor;
using Skylight.API.Game.Rooms;
using Skylight.API.Game.Users;
using Skylight.API.Numerics;

namespace Skylight.Server.Game.Rooms.Items.Floor.Factory;

internal sealed class FurniMaticGiftRoomItemFactory : FloorRoomItemFactory<IFurniMaticGiftFurniture, FurniMaticGiftRoomItem, DateTime>
{
	public override FurniMaticGiftRoomItem Create(IRoom room, int itemId, IUserInfo owner, IFurniMaticGiftFurniture furniture, Point3D position, int direction, DateTime data)
	{
		return new FurniMaticGiftRoomItem(room, itemId, owner, furniture, position, direction, data);
	}

	public override FurniMaticGiftRoomItem Create(IRoom room, int itemId, IUserInfo owner, IFurniMaticGiftFurniture furniture, Point3D position, int direction, JsonDocument? extraData)
	{
		return new FurniMaticGiftRoomItem(room, itemId, owner, furniture, position, direction, extraData?.RootElement.GetDateTime() ?? DateTime.UnixEpoch);
	}
}
