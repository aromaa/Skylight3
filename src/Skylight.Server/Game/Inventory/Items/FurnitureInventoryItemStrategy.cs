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

		this.RegisterBuilder<IFloorFurniture, DefaultFloorInventoryItemBuilder>();
		this.RegisterBuilder<IWallFurniture, DefaultWallInventoryItemBuilder>();
		this.RegisterBuilder<IStickyNoteFurniture, StickyNoteInventoryItemBuilder>();
		this.RegisterBuilder<IFurniMaticGiftFurniture, FurniMaticGiftInventoryItemBuilder>();
		this.RegisterBuilder<ISoundSetFurniture, SoundSetInventoryItemBuilder>();
	}

	private void RegisterBuilder<TFurniture, TBuilder>()
		where TFurniture : IFurniture
		where TBuilder : IFurnitureInventoryItemBuilder
	{
		this.builders.Add(typeof(TFurniture), ActivatorUtilities.CreateFactory(typeof(TBuilder), []));
	}

	private ObjectFactory Get(Type type)
	{
		foreach (Type targetType in Enumerable.Reverse(type.GetInterfaces()))
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

		IFurnitureInventoryItemBuilder<TInventoryItem> itemBuilder = (IFurnitureInventoryItemBuilder<TInventoryItem>)builderFactory.Invoke(this.serviceProvider, []);
		if (extraData is not null)
		{
			itemBuilder.ExtraData(extraData);
		}

		return itemBuilder
			.Furniture(furniture)
			.Owner(owner)
			.Id(itemId)
			.Build();
	}

	public TInventoryItem CreateFurnitureItem<TFurniture, TInventoryItem, TBuilder>(int itemId, IUserInfo owner, TFurniture furniture, Func<TBuilder, IFurnitureItemBuilder<TFurniture, TInventoryItem>> builder)
		where TFurniture : IFurniture
		where TInventoryItem : IFurnitureInventoryItem, IFurnitureItem<TFurniture>
	{
		ObjectFactory builderFactory = this.typeCache.GetOrAdd(furniture.GetType(), static (type, instance) => instance.Get(type), this);

		IFurnitureInventoryItemBuilder<TInventoryItem> itemBuilder = (IFurnitureInventoryItemBuilder<TInventoryItem>)builderFactory.Invoke(this.serviceProvider, []);
		builder((TBuilder)itemBuilder);

		return itemBuilder
			.Furniture(furniture)
			.Owner(owner)
			.Id(itemId)
			.Build();
	}
}
