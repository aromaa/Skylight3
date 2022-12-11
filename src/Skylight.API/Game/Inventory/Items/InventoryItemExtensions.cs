using System.Text.Json;
using Skylight.API.Game.Furniture.Floor;
using Skylight.API.Game.Furniture.Wall;
using Skylight.API.Game.Inventory.Items.Floor;
using Skylight.API.Game.Inventory.Items.Wall;
using Skylight.API.Game.Users;

namespace Skylight.API.Game.Inventory.Items;

public static class InventoryItemExtensions
{
	public static IFloorInventoryItem CreateFurnitureItem(this IFurnitureInventoryItemStrategy strategy, int itemId, IUserInfo owner, IFloorFurniture furniture, JsonDocument? extraData)
	{
		return strategy.CreateFurnitureItem<IFloorFurniture, IFloorInventoryItem>(itemId, owner, furniture, extraData);
	}

	public static IWallInventoryItem CreateFurnitureItem(this IFurnitureInventoryItemStrategy strategy, int itemId, IUserInfo owner, IWallFurniture furniture, JsonDocument? extraData)
	{
		return strategy.CreateFurnitureItem<IWallFurniture, IWallInventoryItem>(itemId, owner, furniture, extraData);
	}
}
