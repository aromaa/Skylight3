using System.Text.Json;
using Skylight.API.Game.Furniture.Floor;
using Skylight.API.Game.Rooms;
using Skylight.API.Game.Rooms.Items.Interactions;
using Skylight.API.Game.Users;
using Skylight.API.Numerics;

namespace Skylight.Server.Game.Rooms.Items.Floor.Factory;

internal sealed class StickyNotePoleRoomItemFactory : FloorRoomItemFactory<IStickyNotePoleFurniture, StickyNotePoleRoomItem>
{
	public override StickyNotePoleRoomItem Create(IRoom room, int itemId, IUserInfo owner, IStickyNotePoleFurniture furniture, Point3D position, int direction, JsonDocument? extraData)
	{
		room.ItemManager.TryGetInteractionHandler(out IStickyNoteInteractionHandler? handler);

		return new StickyNotePoleRoomItem(room, itemId, owner, furniture, position, direction, handler!);
	}
}
