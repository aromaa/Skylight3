using System.Text.Json;
using Skylight.API.Game.Furniture;
using Skylight.API.Game.Furniture.Wall;
using Skylight.API.Game.Rooms.Items.Wall;
using Skylight.API.Game.Rooms.Private;
using Skylight.API.Game.Users;
using Skylight.API.Numerics;

namespace Skylight.Server.Game.Rooms.Items.Wall;

internal sealed class WallRoomItemStrategy<TRoomItem, TFurniture>(IWallRoomItemStrategy wallRoomItemStrategy) : IWallRoomItemStrategy<TRoomItem, TFurniture>
	where TRoomItem : IWallRoomItem, IFurnitureItem<TFurniture>
	where TFurniture : IWallFurniture
{
	private readonly IWallRoomItemStrategy wallRoomItemStrategy = wallRoomItemStrategy;

	public TRoomItem CreateWallItem(int itemId, IPrivateRoom room, IUserInfo owner, TFurniture furniture, Point2D location, Point2D position, JsonDocument? extraData = null)
	{
		return this.wallRoomItemStrategy.CreateWallItem<TRoomItem, TFurniture>(itemId, room, owner, furniture, location, position, extraData);
	}

	public TRoomItem CreateWallItem<TBuilder>(int itemId, IPrivateRoom room, IUserInfo owner, TFurniture furniture, Point2D location, Point2D position, Action<TBuilder> builder)
		where TBuilder : IFurnitureItemDataBuilder<TFurniture, TRoomItem, TBuilder>
	{
		return this.wallRoomItemStrategy.CreateWallItem<TRoomItem, TFurniture, TBuilder>(itemId, room, owner, furniture, location, position, builder);
	}
}
