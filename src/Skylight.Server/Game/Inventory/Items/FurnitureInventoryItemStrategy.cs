using System.Collections.Concurrent;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Skylight.API.Game.Furniture;
using Skylight.API.Game.Furniture.Floor;
using Skylight.API.Game.Furniture.Wall;
using Skylight.API.Game.Inventory.Items;
using Skylight.API.Game.Users;
using Skylight.Server.Game.Inventory.Items.Floor.Builders;
using Skylight.Server.Game.Inventory.Items.Wall.Builders;

namespace Skylight.Server.Game.Inventory.Items;

internal sealed class FurnitureInventoryItemStrategy : IFurnitureInventoryItemStrategy
{
	private readonly IServiceProvider serviceProvider;

	private readonly Dictionary<Type, ObjectFactory> builders = [];
	private readonly ConcurrentDictionary<Type, ObjectFactory> typeCache = [];

	public FurnitureInventoryItemStrategy(IServiceProvider serviceProvider)
	{
		this.serviceProvider = serviceProvider;

		this.RegisterBuilder<IFloorFurniture, DefaultFloorInventoryItemBuilderImpl>();
		this.RegisterBuilder<IWallFurniture, DefaultWallInventoryItemBuilderImpl>();
		this.RegisterBuilder<IStickyNoteFurniture, StickyNoteInventoryItemBuilderImpl>();
		this.RegisterBuilder<IFurniMaticGiftFurniture, FurniMaticGiftInventoryItemBuilderImpl>();
		this.RegisterBuilder<ISoundSetFurniture, SoundSetInventoryItemBuilderImpl>();
	}

	private void RegisterBuilder<TFurniture, TBuilder>()
		where TFurniture : IFurniture
		where TBuilder : FurnitureInventoryItemBuilder
	{
		this.builders.Add(typeof(TFurniture), ActivatorUtilities.CreateFactory(typeof(TBuilder), []));
	}

	private ObjectFactory Get(Type type)
	{
		foreach (Type targetType in type.GetInterfaces().Reverse())
		{
			if (this.builders.TryGetValue(targetType, out ObjectFactory? objectFactory))
			{
				return objectFactory;
			}
		}

		throw new NotSupportedException();
	}

	public TInventoryItem CreateFurnitureItem<TFurniture, TInventoryItem>(int itemId, IUserInfo owner, TFurniture furniture, JsonDocument? extraData = null)
		where TFurniture : IFurniture
		where TInventoryItem : IFurnitureInventoryItem, IFurnitureItem<TFurniture>
	{
		ObjectFactory builderFactory = this.typeCache.GetOrAdd(furniture.GetType(), static (type, instance) => instance.Get(type), this);

		FurnitureInventoryItemBuilder itemBuilder = (FurnitureInventoryItemBuilder)builderFactory.Invoke(this.serviceProvider, []);
		if (extraData is not null)
		{
			itemBuilder.ExtraData(extraData);
		}

		return (TInventoryItem)itemBuilder
			.ItemId(itemId)
			.Owner(owner)
			.Furniture(furniture)
			.Build();
	}

	public TInventoryItem CreateFurnitureItem<TFurniture, TInventoryItem, TBuilder>(int itemId, IUserInfo owner, TFurniture furniture, Func<TBuilder, IFurnitureItemBuilder<TFurniture, TInventoryItem>> builder)
		where TFurniture : IFurniture
		where TInventoryItem : IFurnitureInventoryItem, IFurnitureItem<TFurniture>
	{
		ObjectFactory builderFactory = this.typeCache.GetOrAdd(furniture.GetType(), static (type, instance) => instance.Get(type), this);

		FurnitureInventoryItemBuilder itemBuilder = (FurnitureInventoryItemBuilder)builderFactory.Invoke(this.serviceProvider, []);
		builder((TBuilder)(object)itemBuilder);

		return (TInventoryItem)itemBuilder
			.ItemId(itemId)
			.Owner(owner)
			.Furniture(furniture)
			.Build();
	}
}
