using Skylight.API.Game.Furniture.Floor;
using Skylight.API.Game.Rooms;
using Skylight.API.Game.Rooms.Items.Floor;
using Skylight.API.Game.Rooms.Items.Interactions;
using Skylight.API.Game.Users;
using Skylight.API.Numerics;

namespace Skylight.Server.Game.Rooms.Items.Floor;

internal class RollerRoomItem(IRoom room, int id, IUserInfo owner, IRollerFurniture furniture, Point3D position, int direction, IRollerInteractionHandler interactionHandler)
	: PlainFloorRoomItem<IRollerFurniture>(room, id, owner, furniture, position, direction), IRollerRoomItem
{
	private readonly IRollerInteractionHandler interactionHandler = interactionHandler;

	public new IRollerFurniture Furniture => this.furniture;

	public override void OnPlace() => this.interactionHandler.OnPlace(this);

	public override void OnMove(Point3D position, int direction)
	{
		this.interactionHandler.OnRemove(this);

		base.OnMove(position, direction);

		this.interactionHandler.OnPlace(this);
	}

	public override void OnRemove() => this.interactionHandler.OnRemove(this);
}
