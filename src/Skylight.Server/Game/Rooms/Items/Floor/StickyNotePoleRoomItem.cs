using Skylight.API.Game.Furniture.Floor;
using Skylight.API.Game.Rooms.Items;
using Skylight.API.Game.Rooms.Items.Floor;
using Skylight.API.Game.Rooms.Items.Interactions;
using Skylight.API.Game.Rooms.Private;
using Skylight.API.Game.Users;
using Skylight.API.Numerics;

namespace Skylight.Server.Game.Rooms.Items.Floor;

internal sealed class StickyNotePoleRoomItem(IPrivateRoom room, RoomItemId id, IUserInfo owner, IStickyNotePoleFurniture furniture, Point3D position, int direction, IStickyNoteInteractionHandler interactionHandler)
	: PlainFloorRoomItem<IStickyNotePoleFurniture>(room, id, owner, furniture, position, direction), IStickyNotePoleRoomItem
{
	private readonly IStickyNoteInteractionHandler interactionHandler = interactionHandler;

	public new IStickyNotePoleFurniture Furniture => this.furniture;

	public override void OnPlace()
	{
		this.interactionHandler.OnPlace(this);
	}

	public override void OnRemove()
	{
		this.interactionHandler.OnRemove(this);
	}
}
