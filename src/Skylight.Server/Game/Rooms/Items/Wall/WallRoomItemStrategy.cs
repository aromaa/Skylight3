using System.Collections.Concurrent;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Skylight.API.Game.Furniture;
using Skylight.API.Game.Furniture.Wall;
using Skylight.API.Game.Rooms;
using Skylight.API.Game.Rooms.Items.Wall;
using Skylight.API.Game.Rooms.Items.Wall.Builders;
using Skylight.API.Game.Users;
using Skylight.API.Numerics;
using Skylight.Server.Game.Rooms.Items.Wall.Builders;

namespace Skylight.Server.Game.Rooms.Items.Wall;

internal sealed class WallRoomItemStrategy : IWallRoomItemStrategy
{
	private readonly IServiceProvider serviceProvider;

	private readonly Dictionary<Type, ObjectFactory> builders = [];
	private readonly ConcurrentDictionary<Type, ObjectFactory> typeCache = [];

	public WallRoomItemStrategy(IServiceProvider serviceProvider)
	{
		this.serviceProvider = serviceProvider;

		this.RegisterBuilder<IStaticWallFurniture, StaticWallRoomItemBuilderImpl>();
		this.RegisterBuilder<IStickyNoteFurniture, StickyNoteRoomItemBuilderImpl>();
	}

	private void RegisterBuilder<TFurniture, TBuilder>()
		where TFurniture : IWallFurniture
		where TBuilder : WallRoomItemBuilder
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

	public TRoomItem CreateWallItem<TFurniture, TRoomItem>(int itemId, IRoom room, IUserInfo owner, TFurniture furniture, Point2D location, Point2D position, JsonDocument? extraData = null)
		where TFurniture : IWallFurniture
		where TRoomItem : IWallRoomItem, IFurnitureItem<TFurniture>
	{
		ObjectFactory builderFactory = this.typeCache.GetOrAdd(furniture.GetType(), static (type, instance) => instance.Get(type), this);

		WallRoomItemBuilder itemBuilder = (WallRoomItemBuilder)builderFactory.Invoke(this.serviceProvider, []);
		if (extraData is not null)
		{
			itemBuilder.ExtraData(extraData);
		}

		return (TRoomItem)itemBuilder
			.ItemId(itemId)
			.Room(room)
			.Owner(owner)
			.Furniture(furniture)
			.Location(location)
			.Position(position)
			.Build();
	}

	public TRoomItem CreateWallItem<TFurniture, TRoomItem, TBuilder>(int itemId, IRoom room, IUserInfo owner, TFurniture furniture, Point2D location, Point2D position, Func<TBuilder, IFurnitureItemBuilder<TFurniture, TRoomItem>> builder)
		where TFurniture : IWallFurniture
		where TRoomItem : IWallRoomItem, IFurnitureItem<TFurniture>
	{
		ObjectFactory builderFactory = this.typeCache.GetOrAdd(furniture.GetType(), static (type, instance) => instance.Get(type), this);

		WallRoomItemBuilder itemBuilder = (WallRoomItemBuilder)builderFactory.Invoke(this.serviceProvider, []);
		builder((TBuilder)(object)itemBuilder);

		return (TRoomItem)itemBuilder
			.ItemId(itemId)
			.Room(room)
			.Owner(owner)
			.Furniture(furniture)
			.Location(location)
			.Position(position)
			.Build();
	}
}
