using System.Text.Json;
using Skylight.API.Game.Furniture;
using Skylight.API.Game.Furniture.Floor;
using Skylight.API.Game.Furniture.Wall;
using Skylight.API.Game.Inventory.Items;
using Skylight.API.Game.Users;
using Skylight.Server.Game.Inventory.Items.Floor;
using Skylight.Server.Game.Inventory.Items.Wall;

namespace Skylight.Server.Game.Inventory.Items;

internal sealed class FurnitureInventoryItemStrategy : IFurnitureInventoryItemStrategy
{
	private readonly IFurnitureInventoryItemFactory[] factories;

	public FurnitureInventoryItemStrategy(IEnumerable<IFurnitureInventoryItemFactory> factories)
	{
		this.factories = factories.ToArray();
	}

	public TInventoryItem CreateFurnitureItem<TFurniture, TInventoryItem, TData>(int itemId, IUserInfo owner, TFurniture furniture, TData extraData)
		where TFurniture : IFurniture
		where TInventoryItem : IFurnitureInventoryItem, IFurnitureItem<TFurniture>, IFurnitureData<TData>
	{
		foreach (IFurnitureInventoryItemFactory factory in this.factories)
		{
			if (factory.Supports(furniture))
			{
				return factory.Create<TFurniture, TInventoryItem, TData>(itemId, owner, furniture, extraData);
			}
		}

		if (furniture is IFloorFurniture floorFurniture)
		{
			return (TInventoryItem)(object)new DefaultFloorInventoryItem(itemId, owner, floorFurniture);
		}
		else if (furniture is IWallFurniture wallFurniture)
		{
			return (TInventoryItem)(object)new DefaultWallInventoryItem(itemId, owner, wallFurniture);
		}
		else
		{
			throw new NotSupportedException();
		}
	}

	public TInventoryItem CreateFurnitureItem<TFurniture, TInventoryItem>(int itemId, IUserInfo owner, TFurniture furniture, JsonDocument? extraData)
		where TFurniture : IFurniture
		where TInventoryItem : IFurnitureInventoryItem, IFurnitureItem<TFurniture>
	{
		foreach (IFurnitureInventoryItemFactory factory in this.factories)
		{
			if (factory.Supports(furniture))
			{
				return factory.Create<TFurniture, TInventoryItem>(itemId, owner, furniture, extraData);
			}
		}

		if (furniture is IFloorFurniture floorFurniture)
		{
			return (TInventoryItem)(object)new DefaultFloorInventoryItem(itemId, owner, floorFurniture);
		}
		else if (furniture is IWallFurniture wallFurniture)
		{
			return (TInventoryItem)(object)new DefaultWallInventoryItem(itemId, owner, wallFurniture);
		}
		else
		{
			throw new NotSupportedException();
		}
	}
}
