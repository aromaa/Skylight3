using Skylight.API.Game.Rooms.Items.Floor;
using Skylight.API.Game.Rooms.Items.Wall;

namespace Skylight.API.Game.Rooms.Items.Interactions;

public interface IStickyNoteInteractionHandler : IRoomItemInteractionHandler
{
	public bool HasStickyNotePole { get; }

	public void OnPlace(IStickyNotePoleRoomItem pole);
	public void OnPlace(IStickyNoteRoomItem stickyNote);

	public void OnRemove(IStickyNotePoleRoomItem pole);
	public void OnRemove(IStickyNoteRoomItem stickyNote);
}
