using System.Text.Json;
using Skylight.API.Game.Furniture;
using Skylight.API.Game.Furniture.Floor;
using Skylight.API.Game.Inventory.Items.Floor;
using Skylight.API.Game.Users;
using Skylight.API.Numerics;

namespace Skylight.API.Game.Rooms.Items.Floor;

public interface IFloorRoomItemStrategy
{
	public IFloorRoomItem CreateFloorItem(int itemId, IRoom room, IUserInfo owner, IFloorFurniture furniture, Point3D position, int direction, JsonDocument? extraData = null)
		=> this.CreateFloorItem<IFloorRoomItem, IFloorFurniture>(itemId, room, owner, furniture, position, direction, extraData);

	public TRoomItem CreateFloorItem<TRoomItem, TFurniture>(int itemId, IRoom room, IUserInfo owner, TFurniture furniture, Point3D position, int direction, JsonDocument? extraData = null)
		where TRoomItem : IFloorRoomItem, IFurnitureItem<TFurniture>
		where TFurniture : IFloorFurniture;

	public TRoomItem CreateFloorItem<TRoomItem, TFurniture, TBuilder>(int itemId, IRoom room, IUserInfo owner, TFurniture furniture, Point3D position, int direction, Action<TBuilder> builder)
		where TRoomItem : IFloorRoomItem, IFurnitureItem<TFurniture>
		where TFurniture : IFloorFurniture
		where TBuilder : IFurnitureItemDataBuilder<TFurniture, TRoomItem, TBuilder>;

	public IFloorRoomItem CreateFloorItem(IFloorInventoryItem item, IRoom room, Point3D position, int direction, JsonDocument? extraData = null)
		=> this.CreateFloorItem<IFloorRoomItem, IFloorFurniture>(item.Id, room, item.Owner, item.Furniture, position, direction, extraData);

	public TRoomItem CreateFloorItem<TRoomItem, TFurniture, TInventoryItem>(TInventoryItem item, IRoom room, Point3D position, int direction, JsonDocument? extraData = null)
		where TRoomItem : IFloorRoomItem, IFurnitureItem<TFurniture>
		where TFurniture : IFloorFurniture
		where TInventoryItem : IFloorInventoryItem, IFurnitureItem<TFurniture> => this.CreateFloorItem<TRoomItem, TFurniture>(item.Id, room, item.Owner, ((IFurnitureItem<TFurniture>)item).Furniture, position, direction, extraData);

	public TRoomItem CreateFloorItem<TRoomItem, TFurniture, TInventoryItem, TBuilder>(TInventoryItem item, IRoom room, Point3D position, int direction, Action<TBuilder> builder)
		where TRoomItem : IFloorRoomItem, IFurnitureItem<TFurniture>
		where TFurniture : IFloorFurniture
		where TInventoryItem : IFloorInventoryItem, IFurnitureItem<TFurniture>
		where TBuilder : IFurnitureItemDataBuilder<TFurniture, TRoomItem, TBuilder> => this.CreateFloorItem<TRoomItem, TFurniture, TBuilder>(item.Id, room, item.Owner, ((IFurnitureItem<TFurniture>)item).Furniture, position, direction, builder);
}

public interface IFloorRoomItemStrategy<TRoomItem, TFurniture>
	where TRoomItem : IFloorRoomItem, IFurnitureItem<TFurniture>
	where TFurniture : IFloorFurniture
{
	public TRoomItem CreateFloorItem(int itemId, IRoom room, IUserInfo owner, TFurniture furniture, Point3D position, int direction, JsonDocument? extraData = null);

	public TRoomItem CreateFloorItem<TBuilder>(int itemId, IRoom room, IUserInfo owner, TFurniture furniture, Point3D position, int direction, Action<TBuilder> builder)
		where TBuilder : IFurnitureItemDataBuilder<TFurniture, TRoomItem, TBuilder>;

	public TRoomItem CreateFloorItem<TInventoryItem>(TInventoryItem item, IRoom room, Point3D position, int direction, JsonDocument? extraData = null)
		where TInventoryItem : IFloorInventoryItem, IFurnitureItem<TFurniture> => this.CreateFloorItem(item.Id, room, item.Owner, ((IFurnitureItem<TFurniture>)item).Furniture, position, direction, extraData);

	public TRoomItem CreateFloorItem<TInventoryItem, TBuilder>(TInventoryItem item, IRoom room, Point3D position, int direction, Action<TBuilder> builder)
		where TInventoryItem : IFloorInventoryItem, IFurnitureItem<TFurniture>
		where TBuilder : IFurnitureItemDataBuilder<TFurniture, TRoomItem, TBuilder> => this.CreateFloorItem(item.Id, room, item.Owner, ((IFurnitureItem<TFurniture>)item).Furniture, position, direction, builder);
}
