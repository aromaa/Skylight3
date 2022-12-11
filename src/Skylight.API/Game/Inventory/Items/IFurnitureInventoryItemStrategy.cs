using System.Text.Json;
using Skylight.API.Game.Furniture;
using Skylight.API.Game.Users;

namespace Skylight.API.Game.Inventory.Items;

public interface IFurnitureInventoryItemStrategy
{
	public TInventoryItem CreateFurnitureItem<TFurniture, TInventoryItem, TData>(int itemId, IUserInfo owner, TFurniture furniture, TData extraData)
		where TFurniture : IFurniture
		where TInventoryItem : IFurnitureInventoryItem, IFurnitureItem<TFurniture>, IFurnitureData<TData>;

	public TInventoryItem CreateFurnitureItem<TFurniture, TInventoryItem>(int itemId, IUserInfo owner, TFurniture furniture, JsonDocument? extraData)
		where TFurniture : IFurniture
		where TInventoryItem : IFurnitureInventoryItem, IFurnitureItem<TFurniture>;
}
