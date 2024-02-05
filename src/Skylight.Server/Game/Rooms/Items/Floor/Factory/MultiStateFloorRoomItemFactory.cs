using System.Text.Json;
using Skylight.API.Game.Furniture.Floor;
using Skylight.API.Game.Rooms;
using Skylight.API.Game.Rooms.Items.Floor;
using Skylight.API.Game.Users;
using Skylight.API.Numerics;

namespace Skylight.Server.Game.Rooms.Items.Floor.Factory;

internal sealed class MultiStateFloorRoomItemFactory : FloorRoomItemFactory<IMultiStateFloorFurniture, IMultiStateFloorItem>
{
	public override IMultiStateFloorItem Create(IRoom room, int itemId, IUserInfo owner, IMultiStateFloorFurniture furniture, Point3D position, int direction, JsonDocument? extraData)
	{
		return new MultiStateFloorRoomItem<IMultiStateFloorFurniture>(room, itemId, owner, furniture, position, direction);
	}
}
