using Skylight.API.Game.Furniture.Floor.Wired.Triggers;
using Skylight.API.Game.Rooms;
using Skylight.API.Game.Rooms.Items.Floor.Wired.Triggers;
using Skylight.API.Game.Rooms.Units;
using Skylight.API.Game.Users;
using Skylight.API.Numerics;

namespace Skylight.Server.Game.Rooms.Items.Floor.Wired.Triggers;

internal abstract class WiredTriggerRoomItem(IRoom room, int id, IUserInfo owner, Point3D position, int direction)
	: FloorRoomItem(room, id, owner, position, direction), IWiredTriggerRoomItem
{
	public abstract override IWiredTriggerFurniture Furniture { get; }

	public override double Height => this.Furniture.DefaultHeight;

	public abstract void Interact(IUserRoomUnit unit, int state);
}
