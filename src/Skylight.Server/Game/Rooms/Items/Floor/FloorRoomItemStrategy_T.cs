using System.Text.Json;
using Skylight.API.Game.Furniture;
using Skylight.API.Game.Furniture.Floor;
using Skylight.API.Game.Inventory.Items.Floor;
using Skylight.API.Game.Rooms.Items;
using Skylight.API.Game.Rooms.Items.Floor;
using Skylight.API.Game.Rooms.Private;
using Skylight.API.Game.Users;
using Skylight.API.Numerics;
using Skylight.API.Registry;

namespace Skylight.Server.Game.Rooms.Items.Floor;

internal sealed class FloorRoomItemStrategy<TRoomItem, TFurniture>(IRegistryHolder registryHolder, IFloorRoomItemStrategy floorRoomItemStrategy) : IFloorRoomItemStrategy<TRoomItem, TFurniture>
	where TRoomItem : IFloorRoomItem, IFurnitureItem<TFurniture>
	where TFurniture : IFloorFurniture
{
	private readonly IRoomItemDomain normalRoomItemDomain = RoomItemDomains.Normal.Get(registryHolder);

	private readonly IFloorRoomItemStrategy floorRoomItemStrategy = floorRoomItemStrategy;

	public TRoomItem CreateFloorItem(RoomItemId itemId, IPrivateRoom room, IUserInfo owner, TFurniture furniture, Point3D position, int direction, JsonDocument? extraData = null)
	{
		return this.floorRoomItemStrategy.CreateFloorItem<TRoomItem, TFurniture>(itemId, room, owner, furniture, position, direction, extraData);
	}

	public TRoomItem CreateFloorItem<TBuilder>(RoomItemId itemId, IPrivateRoom room, IUserInfo owner, TFurniture furniture, Point3D position, int direction, Action<TBuilder> builder)
		where TBuilder : IFurnitureItemDataBuilder<TFurniture, TRoomItem, TBuilder>
	{
		return this.floorRoomItemStrategy.CreateFloorItem<TRoomItem, TFurniture, TBuilder>(itemId, room, owner, furniture, position, direction, builder);
	}

	public TRoomItem CreateFloorItem<TInventoryItem>(TInventoryItem item, IPrivateRoom room, Point3D position, int direction, JsonDocument? extraData = null)
		where TInventoryItem : IFloorInventoryItem, IFurnitureItem<TFurniture> => this.CreateFloorItem(new RoomItemId(this.normalRoomItemDomain, item.Id), room, item.Owner, ((IFurnitureItem<TFurniture>)item).Furniture, position, direction, extraData);

	public TRoomItem CreateFloorItem<TInventoryItem, TBuilder>(TInventoryItem item, IPrivateRoom room, Point3D position, int direction, Action<TBuilder> builder)
		where TInventoryItem : IFloorInventoryItem, IFurnitureItem<TFurniture>
		where TBuilder : IFurnitureItemDataBuilder<TFurniture, TRoomItem, TBuilder> => this.CreateFloorItem(new RoomItemId(this.normalRoomItemDomain, item.Id), room, item.Owner, ((IFurnitureItem<TFurniture>)item).Furniture, position, direction, builder);
}
