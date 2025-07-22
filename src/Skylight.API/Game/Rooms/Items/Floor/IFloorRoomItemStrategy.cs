using System.Text.Json;
using Skylight.API.Game.Furniture;
using Skylight.API.Game.Furniture.Floor;
using Skylight.API.Game.Inventory.Items.Floor;
using Skylight.API.Game.Rooms.Private;
using Skylight.API.Game.Users;
using Skylight.API.Numerics;

namespace Skylight.API.Game.Rooms.Items.Floor;

public interface IFloorRoomItemStrategy
{
	public IFloorRoomItem CreateFloorItem(RoomItemId itemId, IPrivateRoom room, IUserInfo owner, IFloorFurniture furniture, Point3D position, int direction, JsonDocument? extraData = null)
		=> this.CreateFloorItem<IFloorRoomItem, IFloorFurniture>(itemId, room, owner, furniture, position, direction, extraData);

	public TRoomItem CreateFloorItem<TRoomItem, TFurniture>(RoomItemId itemId, IPrivateRoom room, IUserInfo owner, TFurniture furniture, Point3D position, int direction, JsonDocument? extraData = null)
		where TRoomItem : IFloorRoomItem, IFurnitureItem<TFurniture>
		where TFurniture : IFloorFurniture;

	public TRoomItem CreateFloorItem<TRoomItem, TFurniture, TBuilder>(RoomItemId itemId, IPrivateRoom room, IUserInfo owner, TFurniture furniture, Point3D position, int direction, Action<TBuilder> builder)
		where TRoomItem : IFloorRoomItem, IFurnitureItem<TFurniture>
		where TFurniture : IFloorFurniture
		where TBuilder : IFurnitureItemDataBuilder<TFurniture, TRoomItem, TBuilder>;

	public IFloorRoomItem CreateFloorItem(IFloorInventoryItem item, IPrivateRoom room, Point3D position, int direction, JsonDocument? extraData = null);

	public TRoomItem CreateFloorItem<TRoomItem, TFurniture, TInventoryItem>(TInventoryItem item, IPrivateRoom room, Point3D position, int direction, JsonDocument? extraData = null)
		where TRoomItem : IFloorRoomItem, IFurnitureItem<TFurniture>
		where TFurniture : IFloorFurniture
		where TInventoryItem : IFloorInventoryItem, IFurnitureItem<TFurniture>;

	public TRoomItem CreateFloorItem<TRoomItem, TFurniture, TInventoryItem, TBuilder>(TInventoryItem item, IPrivateRoom room, Point3D position, int direction, Action<TBuilder> builder)
		where TRoomItem : IFloorRoomItem, IFurnitureItem<TFurniture>
		where TFurniture : IFloorFurniture
		where TInventoryItem : IFloorInventoryItem, IFurnitureItem<TFurniture>
		where TBuilder : IFurnitureItemDataBuilder<TFurniture, TRoomItem, TBuilder>;
}

public interface IFloorRoomItemStrategy<TRoomItem, TFurniture>
	where TRoomItem : IFloorRoomItem, IFurnitureItem<TFurniture>
	where TFurniture : IFloorFurniture
{
	public TRoomItem CreateFloorItem(RoomItemId itemId, IPrivateRoom room, IUserInfo owner, TFurniture furniture, Point3D position, int direction, JsonDocument? extraData = null);

	public TRoomItem CreateFloorItem<TBuilder>(RoomItemId itemId, IPrivateRoom room, IUserInfo owner, TFurniture furniture, Point3D position, int direction, Action<TBuilder> builder)
		where TBuilder : IFurnitureItemDataBuilder<TFurniture, TRoomItem, TBuilder>;

	public TRoomItem CreateFloorItem<TInventoryItem>(TInventoryItem item, IPrivateRoom room, Point3D position, int direction, JsonDocument? extraData = null)
		where TInventoryItem : IFloorInventoryItem, IFurnitureItem<TFurniture>;

	public TRoomItem CreateFloorItem<TInventoryItem, TBuilder>(TInventoryItem item, IPrivateRoom room, Point3D position, int direction, Action<TBuilder> builder)
		where TInventoryItem : IFloorInventoryItem, IFurnitureItem<TFurniture>
		where TBuilder : IFurnitureItemDataBuilder<TFurniture, TRoomItem, TBuilder>;
}
