using System.Text.Json;
using Skylight.API.Game.Furniture.Floor;
using Skylight.API.Game.Rooms;
using Skylight.API.Game.Rooms.Items.Floor;
using Skylight.API.Game.Rooms.Items.Interactions;
using Skylight.API.Game.Users;
using Skylight.API.Numerics;

namespace Skylight.Server.Game.Rooms.Items.Floor.Factory;

internal sealed class RollerRoomItemFactory : FloorRoomItemFactory<IRollerFurniture, IRollerRoomItem>
{
	public override IRollerRoomItem Create(IRoom room, int itemId, IUserInfo owner, IRollerFurniture furniture, Point3D position, int direction, JsonDocument? extraData)
	{
		if (!room.ItemManager.TryGetInteractionHandler(out IRollerInteractionHandler? handler))
		{
			throw new Exception($"{typeof(IRollerInteractionHandler)} not found");
		}

		return new RollerRoomItem(room, itemId, owner, furniture, position, direction, handler);
	}
}
