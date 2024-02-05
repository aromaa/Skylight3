using Skylight.API.Game.Inventory.Items;
using Skylight.API.Game.Inventory.Items.Floor;
using Skylight.API.Game.Inventory.Items.Wall;
using Skylight.API.Game.Rooms.Items;
using Skylight.API.Game.Rooms.Items.Wall;
using Skylight.Protocol.Packets.Data.Room.Object.Data;
using Skylight.Protocol.Packets.Data.Room.Object.Data.Wall;

namespace Skylight.Server.Extensions;

internal static class ItemExtensions
{
	//TODO: Figure out the best way to do this
	//Ideally we want to have contract in the protocol that does the conversion
	//We generally don't want to mix protocol and game logic
	public static IItemData GetItemData(this IInventoryItem item)
	{
		if (item is IStickyNoteInventoryItem stickyNote)
		{
			return new PostItInventoryData(stickyNote.Count);
		}

		return EmptyItemData.Instance;
	}

	//TODO: This has the same issues as the item data
	//Technically we can rely this on more than how the item data works
	//But just using enums feels ugly, at least we don't need to expose this in the API
	public static int GetItemCategory(this IInventoryItem item)
	{
		if (item is IStickyNoteInventoryItem)
		{
			return 5;
		}
		else if (item is IFurniMaticGiftInventoryItem)
		{
			return 10;
		}

		return 1;
	}

	public static IItemData GetItemData(this IRoomItem item)
	{
		if (item is IStickyNoteRoomItem stickyNote)
		{
			return new PostItRoomData(stickyNote.Color, stickyNote.Text);
		}
		else if (item is IMultiStateRoomItem multiState)
		{
			return new LegacyItemData(multiState.State.ToString());
		}

		return EmptyItemData.Instance;
	}
}
