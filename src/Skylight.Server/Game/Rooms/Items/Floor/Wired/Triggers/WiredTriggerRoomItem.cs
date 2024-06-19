using Skylight.API.Game.Furniture.Floor.Wired.Triggers;
using Skylight.API.Game.Rooms;
using Skylight.API.Game.Rooms.Items.Floor.Wired.Triggers;
using Skylight.API.Game.Rooms.Units;
using Skylight.API.Game.Users;
using Skylight.API.Numerics;

namespace Skylight.Server.Game.Rooms.Items.Floor.Wired.Triggers;

internal abstract class WiredTriggerRoomItem<T>(IRoom room, int id, IUserInfo owner, T furniture, Point3D position, int direction)
	: FloorRoomItem<T>(room, id, owner, furniture, position, direction), IWiredTriggerRoomItem
	where T : IWiredTriggerFurniture
{
	public new IWiredTriggerFurniture Furniture => this.furniture;

	public override double Height => this.Furniture.DefaultHeight;

	public abstract void Interact(IUserRoomUnit unit, int state);
}
