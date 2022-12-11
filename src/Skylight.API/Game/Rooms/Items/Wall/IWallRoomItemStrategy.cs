using System.Text.Json;
using Skylight.API.Game.Furniture;
using Skylight.API.Game.Furniture.Wall;
using Skylight.API.Game.Users;
using Skylight.API.Numerics;

namespace Skylight.API.Game.Rooms.Items.Wall;

public interface IWallRoomItemStrategy
{
	public TRoomItem CreateWallItem<TFurniture, TRoomItem, TData>(IRoom room, int itemId, IUserInfo owner, TFurniture furniture, Point2D location, Point2D position, TData data)
		where TFurniture : IWallFurniture
		where TRoomItem : IWallRoomItem, IFurnitureItem<TFurniture>, IFurnitureData<TData>;

	public TRoomItem CreateWallItem<TFurniture, TRoomItem>(IRoom room, int itemId, IUserInfo owner, TFurniture furniture, Point2D location, Point2D position, JsonDocument? extraData)
		where TFurniture : IWallFurniture
		where TRoomItem : IWallRoomItem, IFurnitureItem<TFurniture>;
}
