using System.Drawing;
using System.Text.Json;
using Skylight.API.Game.Furniture.Wall;
using Skylight.API.Game.Inventory.Items.Wall;
using Skylight.API.Game.Rooms.Items.Wall;
using Skylight.API.Game.Users;
using Skylight.API.Numerics;
namespace Skylight.API.Game.Rooms.Items;

public static class WallRoomItemExtensions
{
	public static IWallRoomItem CreateWallItem(this IWallRoomItemStrategy strategy, IRoom room, int itemId, IUserInfo owner, IWallFurniture furniture, Point2D location, Point2D position, JsonDocument? extraData)
	{
		return strategy.CreateWallItem<IWallFurniture, IWallRoomItem>(room, itemId, owner, furniture, location, position, extraData);
	}

	public static IStickyNoteRoomItem CreateWallItem(this IWallRoomItemStrategy strategy, IRoom room, int itemId, IUserInfo owner, IStickyNoteFurniture furniture, Point2D location, Point2D position, Color color, string text = "")
	{
		return strategy.CreateWallItem<IStickyNoteFurniture, IStickyNoteRoomItem, (Color, string)>(room, itemId, owner, furniture, location, position, (color, text));
	}

	public static IWallRoomItem CreateWallItem(this IWallRoomItemStrategy strategy, IRoom room, IWallInventoryItem item, Point2D location, Point2D position)
	{
		return strategy.CreateWallItem(room, item.Id, item.Owner, item.Furniture, location, position, null);
	}

	public static IStickyNoteRoomItem CreateWallItem(this IWallRoomItemStrategy strategy, IRoom room, IStickyNoteInventoryItem item, Point2D location, Point2D position, Color color, string text = "")
	{
		return strategy.CreateWallItem(room, item.Id, item.Owner, item.Furniture, location, position, color, text);
	}
}
