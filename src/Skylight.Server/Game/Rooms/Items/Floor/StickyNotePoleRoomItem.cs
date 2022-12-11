using Skylight.API.Game.Furniture.Floor;
using Skylight.API.Game.Rooms;
using Skylight.API.Game.Rooms.Items.Floor;
using Skylight.API.Game.Rooms.Items.Interactions;
using Skylight.API.Game.Users;
using Skylight.API.Numerics;

namespace Skylight.Server.Game.Rooms.Items.Floor;

internal sealed class StickyNotePoleRoomItem : FloorRoomItem, IStickyNotePoleRoomItem
{
	public override IStickyNotePoleFurniture Furniture { get; }

	private readonly IStickyNoteInteractionHandler interactionHandler;

	public StickyNotePoleRoomItem(IRoom room, int id, IUserInfo owner, IStickyNotePoleFurniture furniture, Point3D position, int direction, IStickyNoteInteractionHandler interactionHandler)
		: base(room, id, owner, position, direction)
	{
		this.Furniture = furniture;

		this.interactionHandler = interactionHandler;
	}

	public override void OnPlace()
	{
		this.interactionHandler.OnPlace(this);
	}

	public override void OnRemove()
	{
		this.interactionHandler.OnRemove(this);
	}
}
