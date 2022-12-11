using System.Text.Json;
using Skylight.API.Game.Furniture;
using Skylight.API.Game.Users;

namespace Skylight.API.Game.Inventory.Items;

//TODO: Hmm. Maybe instead leave just Supports and create a generic interface instead
public interface IFurnitureInventoryItemFactory
{
	public bool Supports(IFurniture furniture);

	public TInventoryItem Create<TFurniture, TInventoryItem, TData>(int itemId, IUserInfo owner, TFurniture furniture, TData extraData)
		where TFurniture : IFurniture
		where TInventoryItem : IFurnitureInventoryItem, IFurnitureItem<TFurniture>, IFurnitureData<TData>;

	public TInventoryItem Create<TFurniture, TInventoryItem>(int itemId, IUserInfo owner, TFurniture furniture, JsonDocument? extraData)
		where TFurniture : IFurniture
		where TInventoryItem : IFurnitureInventoryItem, IFurnitureItem<TFurniture>;
}
