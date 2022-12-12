using System.Text.Json;
using Skylight.API.Game.Furniture;
using Skylight.API.Game.Inventory.Items;
using Skylight.API.Game.Users;

namespace Skylight.Server.Game.Inventory.Items;

internal abstract class FurnitureInventoryItemFactory<TFurniture, TInventoryItem, TData> : IFurnitureInventoryItemFactory
	where TFurniture : IFurniture
	where TInventoryItem : IInventoryItem, IFurnitureItem<TFurniture>, IFurnitureData<TData>
{
	public bool Supports(IFurniture furniture) => furniture is TFurniture;

	public TInventoryItem1 Create<TFurniture1, TInventoryItem1, TData1>(int itemId, IUserInfo owner, TFurniture1 furniture, TData1 extraData)
		where TFurniture1 : IFurniture
		where TInventoryItem1 : IFurnitureInventoryItem, IFurnitureItem<TFurniture1>, IFurnitureData<TData1>
	{
		return (TInventoryItem1)(object)this.Create(itemId, owner, (TFurniture)(object)furniture, (TData)(object)extraData!);
	}

	public TInventoryItem1 Create<TFurniture1, TInventoryItem1>(int itemId, IUserInfo owner, TFurniture1 furniture, JsonDocument? extraData)
		where TFurniture1 : IFurniture
		where TInventoryItem1 : IFurnitureInventoryItem, IFurnitureItem<TFurniture1>
	{
		return (TInventoryItem1)(object)this.Create(itemId, owner, (TFurniture)(object)furniture, extraData);
	}

	public abstract TInventoryItem Create(int itemId, IUserInfo owner, TFurniture furniture, TData extraData);
	public abstract TInventoryItem Create(int itemId, IUserInfo owner, TFurniture furniture, JsonDocument? extraData);
}

internal abstract class FurnitureInventoryItemFactory<TFurniture, TInventoryItem> : IFurnitureInventoryItemFactory
	where TFurniture : IFurniture
	where TInventoryItem : IInventoryItem, IFurnitureItem<TFurniture>
{
	public bool Supports(IFurniture furniture) => furniture is TFurniture;

	public TInventoryItem1 Create<TFurniture1, TInventoryItem1, TData1>(int itemId, IUserInfo owner, TFurniture1 furniture, TData1 extraData)
		where TFurniture1 : IFurniture
		where TInventoryItem1 : IFurnitureInventoryItem, IFurnitureItem<TFurniture1>, IFurnitureData<TData1>
	{
		return (TInventoryItem1)(object)this.Create(itemId, owner, (TFurniture)(object)furniture, null);
	}

	public TInventoryItem1 Create<TFurniture1, TInventoryItem1>(int itemId, IUserInfo owner, TFurniture1 furniture, JsonDocument? extraData)
		where TFurniture1 : IFurniture
		where TInventoryItem1 : IFurnitureInventoryItem, IFurnitureItem<TFurniture1>
	{
		return (TInventoryItem1)(object)this.Create(itemId, owner, (TFurniture)(object)furniture, extraData);
	}

	public abstract TInventoryItem Create(int itemId, IUserInfo owner, TFurniture furniture, JsonDocument? extraData);
}
