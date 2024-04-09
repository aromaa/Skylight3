using System.Collections.Concurrent;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Skylight.API.Game.Furniture;
using Skylight.API.Game.Furniture.Floor;
using Skylight.API.Game.Furniture.Floor.Wired.Effects;
using Skylight.API.Game.Furniture.Floor.Wired.Triggers;
using Skylight.API.Game.Rooms;
using Skylight.API.Game.Rooms.Items.Floor;
using Skylight.API.Game.Rooms.Items.Floor.Builders;
using Skylight.API.Game.Users;
using Skylight.API.Numerics;
using Skylight.Server.Game.Rooms.Items.Floor.Builders;
using Skylight.Server.Game.Rooms.Items.Floor.Wired.Effects.Builders;
using Skylight.Server.Game.Rooms.Items.Floor.Wired.Triggers.Builders;

namespace Skylight.Server.Game.Rooms.Items.Floor;

internal sealed class FloorRoomItemStrategy : IFloorRoomItemStrategy
{
	private readonly IServiceProvider serviceProvider;

	private readonly Dictionary<Type, ObjectFactory> builders = [];
	private readonly ConcurrentDictionary<Type, ObjectFactory> typeCache = [];

	public FloorRoomItemStrategy(IServiceProvider serviceProvider)
	{
		this.serviceProvider = serviceProvider;

		this.RegisterBuilder<IBasicFloorFurniture, BasicFloorRoomItemBuilderImpl>();
		this.RegisterBuilder<IFurniMaticGiftFurniture, FurniMaticGiftRoomItemBuilderImpl>();
		this.RegisterBuilder<IStickyNotePoleFurniture, StickyNotePoleRoomItemBuilderImpl>();
		this.RegisterBuilder<ISoundMachineFurniture, SoundMachineRoomItemBuilderImpl>();
		this.RegisterBuilder<IRollerFurniture, RollerRoomItemBuilderImpl>();
		this.RegisterBuilder<IMultiStateFloorFurniture, MultiStateFloorRoomItemBuilderImpl>();
		this.RegisterBuilder<IUserSayTriggerFurniture, UserSayTriggerRoomItemBuilderImpl>();
		this.RegisterBuilder<IShowMessageEffectFurniture, ShowMessageEffectRoomItemBuilderImpl>();
	}

	private void RegisterBuilder<TFurniture, TBuilder>()
		where TFurniture : IFloorFurniture
		where TBuilder : FloorRoomItemBuilder
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

	public TRoomItem CreateFloorItem<TFurniture, TRoomItem>(int itemId, IRoom room, IUserInfo owner, TFurniture furniture, Point3D position, int direction, JsonDocument? extraData = null)
		where TFurniture : IFloorFurniture
		where TRoomItem : IFloorRoomItem, IFurnitureItem<TFurniture>
	{
		ObjectFactory builderFactory = this.typeCache.GetOrAdd(furniture.GetType(), static (type, instance) => instance.Get(type), this);

		FloorRoomItemBuilder itemBuilder = (FloorRoomItemBuilder)builderFactory.Invoke(this.serviceProvider, []);
		if (extraData is not null)
		{
			itemBuilder.ExtraData(extraData);
		}

		return (TRoomItem)itemBuilder
			.ItemId(itemId)
			.Room(room)
			.Owner(owner)
			.Furniture(furniture)
			.Position(position)
			.Direction(direction)
			.Build();
	}

	public TRoomItem CreateFloorItem<TFurniture, TRoomItem, TBuilder>(int itemId, IRoom room, IUserInfo owner, TFurniture furniture, Point3D position, int direction, Func<TBuilder, IFurnitureItemBuilder<TFurniture, TRoomItem>> builder)
		where TFurniture : IFloorFurniture
		where TRoomItem : IFloorRoomItem, IFurnitureItem<TFurniture>
	{
		ObjectFactory builderFactory = this.typeCache.GetOrAdd(furniture.GetType(), static (type, instance) => instance.Get(type), this);

		FloorRoomItemBuilder itemBuilder = (FloorRoomItemBuilder)builderFactory.Invoke(this.serviceProvider, []);
		builder((TBuilder)(object)itemBuilder);

		return (TRoomItem)itemBuilder
			.ItemId(itemId)
			.Room(room)
			.Owner(owner)
			.Furniture(furniture)
			.Position(position)
			.Direction(direction)
			.Build();
	}
}
