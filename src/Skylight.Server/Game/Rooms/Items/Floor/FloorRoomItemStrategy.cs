using System.Collections.Concurrent;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Skylight.API.Game.Furniture;
using Skylight.API.Game.Furniture.Floor;
using Skylight.API.Game.Furniture.Floor.Wired.Effects;
using Skylight.API.Game.Furniture.Floor.Wired.Triggers;
using Skylight.API.Game.Rooms;
using Skylight.API.Game.Rooms.Items;
using Skylight.API.Game.Rooms.Items.Floor;
using Skylight.API.Game.Users;
using Skylight.API.Numerics;
using Skylight.Server.Game.Rooms.Items.Builders.Floor;
using Skylight.Server.Game.Rooms.Items.Builders.Floor.Wired.Effects;
using Skylight.Server.Game.Rooms.Items.Builders.Floor.Wired.Triggers;

namespace Skylight.Server.Game.Rooms.Items.Floor;

internal sealed class FloorRoomItemStrategy : IFloorRoomItemStrategy
{
	private readonly IServiceProvider serviceProvider;

	private readonly Dictionary<Type, ObjectFactory> builders = [];
	private readonly ConcurrentDictionary<Type, ObjectFactory> typeCache = [];

	public FloorRoomItemStrategy(IServiceProvider serviceProvider)
	{
		this.serviceProvider = serviceProvider;

		this.RegisterBuilder<IStaticFloorFurniture, StaticFloorRoomItemBuilder>();
		this.RegisterBuilder<IFurniMaticGiftFurniture, FurniMaticGiftRoomItemBuilder>();
		this.RegisterBuilder<IStickyNotePoleFurniture, StickyNotePoleRoomItemBuilder>();
		this.RegisterBuilder<ISoundMachineFurniture, SoundMachineRoomItemBuilder>();
		this.RegisterBuilder<IRollerFurniture, RollerRoomItemBuilder>();
		this.RegisterBuilder<IMultiStateFloorFurniture, MultiStateFloorRoomItemBuilder>();
		this.RegisterBuilder<IUnitSayTriggerFurniture, UnitSayTriggerRoomItemBuilder>();
		this.RegisterBuilder<IShowMessageEffectFurniture, ShowMessageEffectRoomItemBuilder>();
		this.RegisterBuilder<IUnitEnterRoomTriggerFurniture, UnitEnterRoomTriggerRoomItemBuilder>();
		this.RegisterBuilder<IUnitUseItemTriggerFurniture, UnitUseItemTriggerRoomItemBuilder>();
		this.RegisterBuilder<ICycleItemStateEffectFurniture, CycleItemStateEffectRoomItemBuilder>();
		this.RegisterBuilder<ITeleportUnitEffectFurniture, TeleportUnitEffectRoomItemBuilder>();
		this.RegisterBuilder<IUnitWalkOffTriggerFurniture, UnitWalkOffTriggerRoomItemBuilder>();
		this.RegisterBuilder<IUnitWalkOnTriggerFurniture, UnitWalkOnTriggerRoomItemBuilder>();
	}

	private void RegisterBuilder<TFurniture, TBuilder>()
		where TFurniture : IFloorFurniture
		where TBuilder : IRoomItemBuilder
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

	public TRoomItem CreateFloorItem<TRoomItem, TFurniture>(int itemId, IRoom room, IUserInfo owner, TFurniture furniture, Point3D position, int direction, JsonDocument? extraData = null)
		where TFurniture : IFloorFurniture
		where TRoomItem : IFloorRoomItem, IFurnitureItem<TFurniture>
	{
		ObjectFactory builderFactory = this.typeCache.GetOrAdd(furniture.GetType(), static (type, instance) => instance.Get(type), this);

		IFloorRoomItemBuilder<TRoomItem> itemBuilder = (IFloorRoomItemBuilder<TRoomItem>)builderFactory.Invoke(this.serviceProvider, []);
		if (extraData is not null)
		{
			itemBuilder.ExtraData(extraData);
		}

		return itemBuilder
			.Position(position)
			.Direction(direction)
			.Room(room)
			.Owner(owner)
			.Furniture(furniture)
			.Id(itemId)
			.Build();
	}

	public TRoomItem CreateFloorItem<TRoomItem, TFurniture, TBuilder>(int itemId, IRoom room, IUserInfo owner, TFurniture furniture, Point3D position, int direction, Action<TBuilder> builder)
		where TRoomItem : IFloorRoomItem, IFurnitureItem<TFurniture>
		where TFurniture : IFloorFurniture
		where TBuilder : IFurnitureItemDataBuilder<TFurniture, TRoomItem, TBuilder>
	{
		ObjectFactory builderFactory = this.typeCache.GetOrAdd(furniture.GetType(), static (type, instance) => instance.Get(type), this);

		IFloorRoomItemBuilder<TRoomItem> itemBuilder = (IFloorRoomItemBuilder<TRoomItem>)builderFactory.Invoke(this.serviceProvider, []);
		builder((TBuilder)itemBuilder);

		return itemBuilder
			.Position(position)
			.Direction(direction)
			.Room(room)
			.Owner(owner)
			.Furniture(furniture)
			.Id(itemId)
			.Build();
	}
}
