using System.Text.Json;
using Skylight.API.Game.Furniture;
using Skylight.API.Game.Furniture.Wall;
using Skylight.API.Game.Inventory.Items.Wall;
using Skylight.API.Game.Rooms.Items;
using Skylight.API.Game.Rooms.Items.Wall;
using Skylight.API.Game.Rooms.Private;
using Skylight.API.Game.Users;
using Skylight.API.Numerics;
using Skylight.API.Registry;

namespace Skylight.Server.Game.Rooms.Items.Wall;

internal sealed class WallRoomItemStrategy<TRoomItem, TFurniture>(IRegistryHolder registryHolder, IWallRoomItemStrategy wallRoomItemStrategy) : IWallRoomItemStrategy<TRoomItem, TFurniture>
	where TRoomItem : IWallRoomItem, IFurnitureItem<TFurniture>
	where TFurniture : IWallFurniture
{
	private readonly IRoomItemDomain normalRoomItemDomain = RoomItemDomains.Normal.Get(registryHolder);

	private readonly IWallRoomItemStrategy wallRoomItemStrategy = wallRoomItemStrategy;

	public TRoomItem CreateWallItem(RoomItemId itemId, IPrivateRoom room, IUserInfo owner, TFurniture furniture, Point2D location, Point2D position, JsonDocument? extraData = null)
	{
		return this.wallRoomItemStrategy.CreateWallItem<TRoomItem, TFurniture>(itemId, room, owner, furniture, location, position, extraData);
	}

	public TRoomItem CreateWallItem<TBuilder>(RoomItemId itemId, IPrivateRoom room, IUserInfo owner, TFurniture furniture, Point2D location, Point2D position, Action<TBuilder> builder)
		where TBuilder : IFurnitureItemDataBuilder<TFurniture, TRoomItem, TBuilder>
	{
		return this.wallRoomItemStrategy.CreateWallItem<TRoomItem, TFurniture, TBuilder>(itemId, room, owner, furniture, location, position, builder);
	}

	public TRoomItem CreateWallItem<TInventoryItem>(TInventoryItem item, IPrivateRoom room, Point2D location, Point2D position, JsonDocument? extraData = null)
		where TInventoryItem : IWallInventoryItem, IFurnitureItem<TFurniture> => this.CreateWallItem(new RoomItemId(this.normalRoomItemDomain, item.Id), room, item.Owner, ((IFurnitureItem<TFurniture>)item).Furniture, location, position, extraData);

	public TRoomItem CreateWallItem<TInventoryItem, TBuilder>(TInventoryItem item, IPrivateRoom room, Point2D location, Point2D position, Action<TBuilder> builder)
		where TInventoryItem : IWallInventoryItem, IFurnitureItem<TFurniture>
		where TBuilder : IFurnitureItemDataBuilder<TFurniture, TRoomItem, TBuilder> => this.CreateWallItem(new RoomItemId(this.normalRoomItemDomain, item.Id), room, item.Owner, ((IFurnitureItem<TFurniture>)item).Furniture, location, position, builder);
}
