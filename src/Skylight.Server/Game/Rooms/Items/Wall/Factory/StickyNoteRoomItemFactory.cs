using System.Drawing;
using System.Text.Json;
using Skylight.API.Game.Furniture.Wall;
using Skylight.API.Game.Rooms;
using Skylight.API.Game.Rooms.Items.Interactions;
using Skylight.API.Game.Rooms.Items.Wall;
using Skylight.API.Game.Users;
using Skylight.API.Numerics;

namespace Skylight.Server.Game.Rooms.Items.Wall.Factory;

internal sealed class StickyNoteRoomItemFactory : WallRoomItemFactory<IStickyNoteFurniture, IStickyNoteRoomItem, (Color Color, string Text)>
{
	public override IStickyNoteRoomItem Create(IRoom room, int itemId, IUserInfo owner, IStickyNoteFurniture furniture, Point2D location, Point2D position, (Color Color, string Text) data)
	{
		if (!room.ItemManager.TryGetInteractionHandler(out IStickyNoteInteractionHandler? handler))
		{
			throw new Exception("StickyNoteInteractionHandler not found");
		}

		return new PostItRoomItem(room, itemId, owner, furniture, location, position, 0, data.Color, data.Text, handler);
	}

	public override IStickyNoteRoomItem Create(IRoom room, int itemId, IUserInfo owner, IStickyNoteFurniture furniture, Point2D location, Point2D position, JsonDocument? extraData)
	{
		Color color = Color.FromArgb(extraData!.RootElement.GetProperty("Color").GetInt32());
		string text = extraData.RootElement.GetProperty("Text").GetString()!;

		return this.Create(room, itemId, owner, furniture, location, position, (color, text));
	}
}
