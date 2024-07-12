using System.Text.Json;
using Skylight.API.Game.Furniture;
using Skylight.API.Game.Furniture.Floor;
using Skylight.API.Game.Rooms.Items.Floor;
using Skylight.API.Game.Rooms.Private;
using Skylight.API.Game.Users;
using Skylight.API.Numerics;

namespace Skylight.Server.Game.Rooms.Items.Floor;

internal sealed class FloorRoomItemStrategy<TRoomItem, TFurniture>(IFloorRoomItemStrategy floorRoomItemStrategy) : IFloorRoomItemStrategy<TRoomItem, TFurniture>
	where TRoomItem : IFloorRoomItem, IFurnitureItem<TFurniture>
	where TFurniture : IFloorFurniture
{
	private readonly IFloorRoomItemStrategy floorRoomItemStrategy = floorRoomItemStrategy;

	public TRoomItem CreateFloorItem(int itemId, IPrivateRoom room, IUserInfo owner, TFurniture furniture, Point3D position, int direction, JsonDocument? extraData = null)
	{
		return this.floorRoomItemStrategy.CreateFloorItem<TRoomItem, TFurniture>(itemId, room, owner, furniture, position, direction, extraData);
	}

	public TRoomItem CreateFloorItem<TBuilder>(int itemId, IPrivateRoom room, IUserInfo owner, TFurniture furniture, Point3D position, int direction, Action<TBuilder> builder)
		where TBuilder : IFurnitureItemDataBuilder<TFurniture, TRoomItem, TBuilder>
	{
		return this.floorRoomItemStrategy.CreateFloorItem<TRoomItem, TFurniture, TBuilder>(itemId, room, owner, furniture, position, direction, builder);
	}
}
