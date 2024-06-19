using System.Collections.Concurrent;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Skylight.API.Game.Furniture;
using Skylight.API.Game.Furniture.Wall;
using Skylight.API.Game.Rooms;
using Skylight.API.Game.Rooms.Items.Wall;
using Skylight.API.Game.Users;
using Skylight.API.Numerics;
using Skylight.Server.Game.Rooms.Items.Builders.Wall;

namespace Skylight.Server.Game.Rooms.Items.Wall;

internal sealed class WallRoomItemStrategy : IWallRoomItemStrategy
{
	private readonly IServiceProvider serviceProvider;

	private readonly Dictionary<Type, ObjectFactory> builders = [];
	private readonly ConcurrentDictionary<Type, ObjectFactory> typeCache = [];

	public WallRoomItemStrategy(IServiceProvider serviceProvider)
	{
		this.serviceProvider = serviceProvider;

		this.RegisterBuilder<IStaticWallFurniture, StaticWallRoomItemBuilder>();
		this.RegisterBuilder<IStickyNoteFurniture, StickyNoteRoomItemBuilder>();
	}

	private void RegisterBuilder<TFurniture, TBuilder>()
		where TFurniture : IWallFurniture
		where TBuilder : IWallRoomItemBuilder
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

	public TRoomItem CreateWallItem<TRoomItem, TFurniture>(int itemId, IRoom room, IUserInfo owner, TFurniture furniture, Point2D location, Point2D position, JsonDocument? extraData = null)
		where TRoomItem : IWallRoomItem, IFurnitureItem<TFurniture>
		where TFurniture : IWallFurniture
	{
		ObjectFactory builderFactory = this.typeCache.GetOrAdd(furniture.GetType(), static (type, instance) => instance.Get(type), this);

		IWallRoomItemBuilder<TRoomItem> itemBuilder = (IWallRoomItemBuilder<TRoomItem>)builderFactory.Invoke(this.serviceProvider, []);
		if (extraData is not null)
		{
			itemBuilder.ExtraData(extraData);
		}

		return itemBuilder
			.Location(location)
			.Position(position)
			.Room(room)
			.Furniture(furniture)
			.Owner(owner)
			.Id(itemId)
			.Build();
	}

	public TRoomItem CreateWallItem<TRoomItem, TFurniture, TBuilder>(int itemId, IRoom room, IUserInfo owner, TFurniture furniture, Point2D location, Point2D position, Action<TBuilder> builder)
		where TRoomItem : IWallRoomItem, IFurnitureItem<TFurniture>
		where TFurniture : IWallFurniture
		where TBuilder : IFurnitureItemDataBuilder<TFurniture, TRoomItem, TBuilder>
	{
		ObjectFactory builderFactory = this.typeCache.GetOrAdd(furniture.GetType(), static (type, instance) => instance.Get(type), this);

		IWallRoomItemBuilder<TRoomItem> itemBuilder = (IWallRoomItemBuilder<TRoomItem>)builderFactory.Invoke(this.serviceProvider, []);
		builder((TBuilder)itemBuilder);

		return itemBuilder
			.Location(location)
			.Position(position)
			.Room(room)
			.Furniture(furniture)
			.Owner(owner)
			.Id(itemId)
			.Build();
	}
}
