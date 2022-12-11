using System.Text.Json;
using Skylight.API.Game.Furniture.Floor;
using Skylight.API.Game.Inventory.Items.Floor;
using Skylight.API.Game.Rooms.Items.Floor;
using Skylight.API.Game.Users;
using Skylight.API.Numerics;

namespace Skylight.API.Game.Rooms.Items;

public static class FloorRoomItemExtensions
{
	public static IFloorRoomItem CreateFloorItem(this IFloorRoomItemStrategy strategy, IRoom room, int itemId, IUserInfo owner, IFloorFurniture furniture, Point3D position, int direction, JsonDocument? extraData)
	{
		return strategy.CreateFloorItem<IFloorFurniture, IFloorRoomItem>(room, itemId, owner, furniture, position, direction, extraData);
	}

	public static IFloorRoomItem CreateFloorItem(this IFloorRoomItemStrategy strategy, IRoom room, IFloorInventoryItem item, Point3D position, int direction)
	{
		return strategy.CreateFloorItem<IFloorFurniture, IFloorRoomItem>(room, item.Id, item.Owner, item.Furniture, position, direction, null);
	}
}
