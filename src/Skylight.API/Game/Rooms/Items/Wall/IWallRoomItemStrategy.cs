using System.Text.Json;
using Skylight.API.Game.Furniture;
using Skylight.API.Game.Furniture.Wall;
using Skylight.API.Game.Inventory.Items.Wall;
using Skylight.API.Game.Users;
using Skylight.API.Numerics;

namespace Skylight.API.Game.Rooms.Items.Wall;

public interface IWallRoomItemStrategy
{
	public IWallRoomItem CreateWallItem(int itemId, IRoom room, IUserInfo owner, IWallFurniture furniture, Point2D location, Point2D position, JsonDocument? extraData = null)
		=> this.CreateWallItem<IWallFurniture, IWallRoomItem>(itemId, room, owner, furniture, location, position, extraData);

	public TRoomItem CreateWallItem<TFurniture, TRoomItem>(int itemId, IRoom room, IUserInfo owner, TFurniture furniture, Point2D location, Point2D position, JsonDocument? extraData = null)
		where TFurniture : IWallFurniture
		where TRoomItem : IWallRoomItem, IFurnitureItem<TFurniture>;

	public TRoomItem CreateWallItem<TFurniture, TRoomItem, TBuilder>(int itemId, IRoom room, IUserInfo owner, TFurniture furniture, Point2D location, Point2D position, Func<TBuilder, IFurnitureItemDataBuilder<TFurniture, TRoomItem, TBuilder>> builder)
		where TFurniture : IWallFurniture
		where TRoomItem : IWallRoomItem, IFurnitureItem<TFurniture>
		where TBuilder : IFurnitureItemDataBuilder<TFurniture, TRoomItem, TBuilder>;

	public IWallRoomItem CreateWallItem(IWallInventoryItem item, IRoom room, Point2D location, Point2D position, JsonDocument? extraData = null)
		=> this.CreateWallItem<IWallFurniture, IWallRoomItem>(item.Id, room, item.Owner, item.Furniture, location, position, extraData);

	public TRoomItem CreateWallItem<TInventoryItem, TFurniture, TRoomItem>(TInventoryItem item, IRoom room, Point2D location, Point2D position, JsonDocument? extraData = null)
		where TInventoryItem : IWallInventoryItem, IFurnitureItem<TFurniture>
		where TFurniture : IWallFurniture
		where TRoomItem : IWallRoomItem, IFurnitureItem<TFurniture> => this.CreateWallItem<TFurniture, TRoomItem>(item.Id, room, item.Owner, ((IFurnitureItem<TFurniture>)item).Furniture, location, position, extraData);

	public TRoomItem CreateWallItem<TInventoryItem, TFurniture, TRoomItem, TBuilder>(TInventoryItem item, IRoom room, Point2D location, Point2D position, Func<TBuilder, IFurnitureItemDataBuilder<TFurniture, TRoomItem, TBuilder>> builder)
		where TInventoryItem : IWallInventoryItem, IFurnitureItem<TFurniture>
		where TFurniture : IWallFurniture
		where TRoomItem : IWallRoomItem, IFurnitureItem<TFurniture>
		where TBuilder : IFurnitureItemDataBuilder<TFurniture, TRoomItem, TBuilder> => this.CreateWallItem(item.Id, room, item.Owner, ((IFurnitureItem<TFurniture>)item).Furniture, location, position, builder);
}
