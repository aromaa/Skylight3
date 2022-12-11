using Skylight.API.Game.Furniture;
using Skylight.API.Game.Furniture.Wall;
using Skylight.API.Game.Rooms.Items.Floor;
using Skylight.API.Game.Rooms.Items.Interactions;
using Skylight.API.Game.Rooms.Items.Wall;
using Skylight.API.Numerics;

namespace Skylight.Server.Game.Rooms.Items.Interactions;

internal sealed class StickyNoteInteractionHandler : IStickyNoteInteractionHandler
{
	private int stickyNoteCount;
	private int stickyNotePoleCount;

	public StickyNoteInteractionHandler()
	{
	}

	public bool HasStickyNotePole => this.stickyNotePoleCount > 0;

	public bool CanPlaceItem(IFurniture furniture, Point2D location)
	{
		if (furniture is IStickyNoteFurniture && this.stickyNoteCount >= 30)
		{
			return false;
		}

		return true;
	}

	public void OnPlace(IStickyNotePoleRoomItem pole) => this.stickyNotePoleCount++;
	public void OnPlace(IStickyNoteRoomItem stickyNote) => this.stickyNoteCount++;

	public void OnRemove(IStickyNotePoleRoomItem pole) => this.stickyNotePoleCount--;
	public void OnRemove(IStickyNoteRoomItem stickyNote) => this.stickyNoteCount--;
}
