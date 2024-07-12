using System.Text.Json;
using Skylight.API.Game.Furniture;
using Skylight.API.Game.Furniture.Wall;
using Skylight.API.Game.Inventory.Items.Wall;
using Skylight.API.Game.Rooms.Private;
using Skylight.API.Game.Users;
using Skylight.API.Numerics;

namespace Skylight.API.Game.Rooms.Items.Wall;

public interface IWallRoomItemStrategy
{
	public IWallRoomItem CreateWallItem(int itemId, IPrivateRoom room, IUserInfo owner, IWallFurniture furniture, Point2D location, Point2D position, JsonDocument? extraData = null)
		=> this.CreateWallItem<IWallRoomItem, IWallFurniture>(itemId, room, owner, furniture, location, position, extraData);

	public TRoomItem CreateWallItem<TRoomItem, TFurniture>(int itemId, IPrivateRoom room, IUserInfo owner, TFurniture furniture, Point2D location, Point2D position, JsonDocument? extraData = null)
		where TRoomItem : IWallRoomItem, IFurnitureItem<TFurniture>
		where TFurniture : IWallFurniture;

	public TRoomItem CreateWallItem<TRoomItem, TFurniture, TBuilder>(int itemId, IPrivateRoom room, IUserInfo owner, TFurniture furniture, Point2D location, Point2D position, Action<TBuilder> builder)
		where TRoomItem : IWallRoomItem, IFurnitureItem<TFurniture>
		where TFurniture : IWallFurniture
		where TBuilder : IFurnitureItemDataBuilder<TFurniture, TRoomItem, TBuilder>;

	public IWallRoomItem CreateWallItem(IWallInventoryItem item, IPrivateRoom room, Point2D location, Point2D position, JsonDocument? extraData = null)
		=> this.CreateWallItem<IWallRoomItem, IWallFurniture>(item.Id, room, item.Owner, item.Furniture, location, position, extraData);

	public TRoomItem CreateWallItem<TRoomItem, TFurniture, TInventoryItem>(TInventoryItem item, IPrivateRoom room, Point2D location, Point2D position, JsonDocument? extraData = null)
		where TRoomItem : IWallRoomItem, IFurnitureItem<TFurniture>
		where TFurniture : IWallFurniture
		where TInventoryItem : IWallInventoryItem, IFurnitureItem<TFurniture> => this.CreateWallItem<TRoomItem, TFurniture>(item.Id, room, item.Owner, ((IFurnitureItem<TFurniture>)item).Furniture, location, position, extraData);

	public TRoomItem CreateWallItem<TRoomItem, TFurniture, TInventoryItem, TBuilder>(TInventoryItem item, IPrivateRoom room, Point2D location, Point2D position, Action<TBuilder> builder)
		where TRoomItem : IWallRoomItem, IFurnitureItem<TFurniture>
		where TFurniture : IWallFurniture
		where TInventoryItem : IWallInventoryItem, IFurnitureItem<TFurniture>
		where TBuilder : IFurnitureItemDataBuilder<TFurniture, TRoomItem, TBuilder> => this.CreateWallItem<TRoomItem, TFurniture, TBuilder>(item.Id, room, item.Owner, ((IFurnitureItem<TFurniture>)item).Furniture, location, position, builder);
}

public interface IWallRoomItemStrategy<TRoomItem, TFurniture>
	where TRoomItem : IWallRoomItem, IFurnitureItem<TFurniture>
	where TFurniture : IWallFurniture
{
	public TRoomItem CreateWallItem(int itemId, IPrivateRoom room, IUserInfo owner, TFurniture furniture, Point2D location, Point2D position, JsonDocument? extraData = null);

	public TRoomItem CreateWallItem<TBuilder>(int itemId, IPrivateRoom room, IUserInfo owner, TFurniture furniture, Point2D location, Point2D position, Action<TBuilder> builder)
		where TBuilder : IFurnitureItemDataBuilder<TFurniture, TRoomItem, TBuilder>;

	public TRoomItem CreateWallItem<TInventoryItem>(TInventoryItem item, IPrivateRoom room, Point2D location, Point2D position, JsonDocument? extraData = null)
		where TInventoryItem : IWallInventoryItem, IFurnitureItem<TFurniture> => this.CreateWallItem(item.Id, room, item.Owner, ((IFurnitureItem<TFurniture>)item).Furniture, location, position, extraData);

	public TRoomItem CreateWallItem<TInventoryItem, TBuilder>(TInventoryItem item, IPrivateRoom room, Point2D location, Point2D position, Action<TBuilder> builder)
		where TInventoryItem : IWallInventoryItem, IFurnitureItem<TFurniture>
		where TBuilder : IFurnitureItemDataBuilder<TFurniture, TRoomItem, TBuilder> => this.CreateWallItem(item.Id, room, item.Owner, ((IFurnitureItem<TFurniture>)item).Furniture, location, position, builder);
}
