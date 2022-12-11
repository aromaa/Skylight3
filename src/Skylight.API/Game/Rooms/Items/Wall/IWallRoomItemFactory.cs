using System.Text.Json;
using Skylight.API.Game.Furniture;
using Skylight.API.Game.Furniture.Wall;
using Skylight.API.Game.Users;
using Skylight.API.Numerics;

namespace Skylight.API.Game.Rooms.Items.Wall;

//TODO: Hmm. Maybe instead leave just Supports and create a generic interface instead
public interface IWallRoomItemFactory
{
	public bool Supports(IWallFurniture furniture);

	public TRoomItem Create<TFurniture, TRoomItem, TData>(IRoom room, int itemId, IUserInfo owner, TFurniture furniture, Point2D location, Point2D position, TData data)
		where TFurniture : IWallFurniture
		where TRoomItem : IWallRoomItem, IFurnitureItem<TFurniture>, IFurnitureData<TData>;

	public TRoomItem Create<TFurniture, TRoomItem>(IRoom room, int itemId, IUserInfo owner, TFurniture furniture, Point2D location, Point2D position, JsonDocument? extraData)
		where TFurniture : IWallFurniture
		where TRoomItem : IWallRoomItem, IFurnitureItem<TFurniture>;
}
