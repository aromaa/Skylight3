using System.Text.Json;
using Skylight.API.Game.Furniture;
using Skylight.API.Game.Furniture.Floor;
using Skylight.API.Game.Furniture.Wall;
using Skylight.API.Game.Inventory.Items.Floor;
using Skylight.API.Game.Inventory.Items.Wall;
using Skylight.API.Game.Users;

namespace Skylight.API.Game.Inventory.Items;

public interface IFurnitureInventoryItemStrategy
{
	public IFurnitureInventoryItem CreateFurnitureItem(int itemId, IUserInfo owner, IFurniture furniture, JsonDocument? extraData = null)
		=> this.CreateFurnitureItem<IFurniture, IFurnitureInventoryItem>(itemId, owner, furniture, extraData);

	public IWallInventoryItem CreateFurnitureItem(int itemId, IUserInfo owner, IWallFurniture furniture, JsonDocument? extraData = null)
		=> this.CreateFurnitureItem<IWallFurniture, IWallInventoryItem>(itemId, owner, furniture, extraData);

	public IFloorInventoryItem CreateFurnitureItem(int itemId, IUserInfo owner, IFloorFurniture furniture, JsonDocument? extraData = null)
		=> this.CreateFurnitureItem<IFloorFurniture, IFloorInventoryItem>(itemId, owner, furniture, extraData);

	public TInventoryItem CreateFurnitureItem<TFurniture, TInventoryItem>(int itemId, IUserInfo owner, TFurniture furniture, JsonDocument? extraData = null)
		where TFurniture : IFurniture
		where TInventoryItem : IFurnitureInventoryItem, IFurnitureItem<TFurniture>;

	public TInventoryItem CreateFurnitureItem<TFurniture, TInventoryItem, TBuilder>(int itemId, IUserInfo owner, TFurniture furniture, Func<TBuilder, IFurnitureItemBuilder<TFurniture, TInventoryItem>> builder)
		where TFurniture : IFurniture
		where TInventoryItem : IFurnitureInventoryItem, IFurnitureItem<TFurniture>;
}
