using System.Text.Json;
using Skylight.API.Game.Furniture;
using Skylight.API.Game.Furniture.Floor;
using Skylight.API.Game.Users;
using Skylight.API.Numerics;

namespace Skylight.API.Game.Rooms.Items.Floor;

//TODO: Hmm. Maybe instead leave just Supports and create a generic interface instead
public interface IFloorRoomItemFactory
{
	public bool Supports(IFloorFurniture furniture);

	public TRoomItem Create<TFurniture, TRoomItem, TData>(IRoom room, int itemId, IUserInfo owner, TFurniture furniture, Point3D position, int direction, TData data)
		where TFurniture : IFloorFurniture
		where TRoomItem : IFloorRoomItem, IFurnitureItem<TFurniture>, IFurnitureData<TData>;

	public TRoomItem Create<TFurniture, TRoomItem>(IRoom room, int itemId, IUserInfo owner, TFurniture furniture, Point3D position, int direction, JsonDocument? extraData)
		where TFurniture : IFloorFurniture
		where TRoomItem : IFloorRoomItem, IFurnitureItem<TFurniture>;
}
