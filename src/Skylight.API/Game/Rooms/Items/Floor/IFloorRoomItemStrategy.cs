using System.Text.Json;
using Skylight.API.Game.Furniture;
using Skylight.API.Game.Furniture.Floor;
using Skylight.API.Game.Users;
using Skylight.API.Numerics;

namespace Skylight.API.Game.Rooms.Items.Floor;

public interface IFloorRoomItemStrategy
{
	public TRoomItem CreateFloorItem<TFurniture, TRoomItem, TData>(IRoom room, int itemId, IUserInfo owner, TFurniture furniture, Point3D position, int direction, TData data)
		where TFurniture : IFloorFurniture
		where TRoomItem : IFloorRoomItem, IFurnitureItem<TFurniture>, IFurnitureData<TData>;

	public TRoomItem CreateFloorItem<TFurniture, TRoomItem>(IRoom room, int itemId, IUserInfo owner, TFurniture furniture, Point3D position, int direction, JsonDocument? extraData)
		where TFurniture : IFloorFurniture
		where TRoomItem : IFloorRoomItem, IFurnitureItem<TFurniture>;
}
