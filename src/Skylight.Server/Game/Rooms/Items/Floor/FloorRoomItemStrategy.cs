using System.Collections.Concurrent;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Skylight.API.Game.Furniture;
using Skylight.API.Game.Furniture.Floor;
using Skylight.API.Game.Furniture.Floor.Wired.Effects;
using Skylight.API.Game.Furniture.Floor.Wired.Triggers;
using Skylight.API.Game.Inventory.Items.Floor;
using Skylight.API.Game.Rooms.Items;
using Skylight.API.Game.Rooms.Items.Floor;
using Skylight.API.Game.Rooms.Private;
using Skylight.API.Game.Users;
using Skylight.API.Numerics;
using Skylight.API.Registry;
using Skylight.Server.Game.Rooms.Items.Builders.Floor;
using Skylight.Server.Game.Rooms.Items.Builders.Floor.Wired.Effects;
using Skylight.Server.Game.Rooms.Items.Builders.Floor.Wired.Triggers;

namespace Skylight.Server.Game.Rooms.Items.Floor;

internal sealed class FloorRoomItemStrategy : IFloorRoomItemStrategy
{
	private readonly IServiceProvider serviceProvider;

	private readonly IRoomItemDomain normalRoomItemDomain;

	private readonly Dictionary<Type, ObjectFactory> builders = [];
	private readonly ConcurrentDictionary<Type, ObjectFactory> typeCache = [];

	public FloorRoomItemStrategy(IServiceProvider serviceProvider, IRegistryHolder registryHolder)
	{
		this.serviceProvider = serviceProvider;

		this.normalRoomItemDomain = RoomItemDomains.Normal.Get(registryHolder);

		this.RegisterBuilder<IStaticFloorFurniture, StaticFloorRoomItemBuilder>();
		this.RegisterBuilder<IFurniMaticGiftFurniture, FurniMaticGiftRoomItemBuilder>();
		this.RegisterBuilder<IStickyNotePoleFurniture, StickyNotePoleRoomItemBuilder>();
		this.RegisterBuilder<ISoundMachineFurniture, SoundMachineRoomItemBuilder>();
		this.RegisterBuilder<IRollerFurniture, RollerRoomItemBuilder>();
		this.RegisterBuilder<IBasicFloorFurniture, BasicFloorRoomItemBuilder>();
		this.RegisterBuilder<IUnitSayTriggerFurniture, UnitSayTriggerRoomItemBuilder>();
		this.RegisterBuilder<IShowMessageEffectFurniture, ShowMessageEffectRoomItemBuilder>();
		this.RegisterBuilder<IUnitEnterRoomTriggerFurniture, UnitEnterRoomTriggerRoomItemBuilder>();
		this.RegisterBuilder<IUnitUseItemTriggerFurniture, UnitUseItemTriggerRoomItemBuilder>();
		this.RegisterBuilder<ICycleItemStateEffectFurniture, CycleItemStateEffectRoomItemBuilder>();
		this.RegisterBuilder<ITeleportUnitEffectFurniture, TeleportUnitEffectRoomItemBuilder>();
		this.RegisterBuilder<IUnitWalkOffTriggerFurniture, UnitWalkOffTriggerRoomItemBuilder>();
		this.RegisterBuilder<IUnitWalkOnTriggerFurniture, UnitWalkOnTriggerRoomItemBuilder>();
		this.RegisterBuilder<IVariableHeightFurniture, VariableHeightRoomItemBuilder>();
	}

	private void RegisterBuilder<TFurniture, TBuilder>()
		where TFurniture : IFloorFurniture
		where TBuilder : IRoomItemBuilder
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

	public TRoomItem CreateFloorItem<TRoomItem, TFurniture>(RoomItemId itemId, IPrivateRoom room, IUserInfo owner, TFurniture furniture, Point3D position, int direction, JsonDocument? extraData = null)
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

	public TRoomItem CreateFloorItem<TRoomItem, TFurniture, TBuilder>(RoomItemId itemId, IPrivateRoom room, IUserInfo owner, TFurniture furniture, Point3D position, int direction, Action<TBuilder> builder)
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

	public IFloorRoomItem CreateFloorItem(IFloorInventoryItem item, IPrivateRoom room, Point3D position, int direction, JsonDocument? extraData = null)
		=> this.CreateFloorItem<IFloorRoomItem, IFloorFurniture>(new RoomItemId(this.normalRoomItemDomain, item.Id), room, item.Owner, item.Furniture, position, direction, extraData);

	public TRoomItem CreateFloorItem<TRoomItem, TFurniture, TInventoryItem>(TInventoryItem item, IPrivateRoom room, Point3D position, int direction, JsonDocument? extraData = null)
		where TRoomItem : IFloorRoomItem, IFurnitureItem<TFurniture>
		where TFurniture : IFloorFurniture
		where TInventoryItem : IFloorInventoryItem, IFurnitureItem<TFurniture>
		=> this.CreateFloorItem<TRoomItem, TFurniture>(new RoomItemId(this.normalRoomItemDomain, item.Id), room, item.Owner, ((IFurnitureItem<TFurniture>)item).Furniture, position, direction, extraData);

	public TRoomItem CreateFloorItem<TRoomItem, TFurniture, TInventoryItem, TBuilder>(TInventoryItem item, IPrivateRoom room, Point3D position, int direction, Action<TBuilder> builder)
		where TRoomItem : IFloorRoomItem, IFurnitureItem<TFurniture>
		where TFurniture : IFloorFurniture
		where TInventoryItem : IFloorInventoryItem, IFurnitureItem<TFurniture>
		where TBuilder : IFurnitureItemDataBuilder<TFurniture, TRoomItem, TBuilder>
		=> this.CreateFloorItem<TRoomItem, TFurniture, TBuilder>(new RoomItemId(this.normalRoomItemDomain, item.Id), room, item.Owner, ((IFurnitureItem<TFurniture>)item).Furniture, position, direction, builder);
}
