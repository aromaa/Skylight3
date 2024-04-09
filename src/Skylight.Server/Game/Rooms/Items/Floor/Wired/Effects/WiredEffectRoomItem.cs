using Skylight.API.Game.Furniture.Floor.Wired.Effects;
using Skylight.API.Game.Rooms;
using Skylight.API.Game.Rooms.Items.Floor.Wired.Effects;
using Skylight.API.Game.Rooms.Units;
using Skylight.API.Game.Users;
using Skylight.API.Numerics;

namespace Skylight.Server.Game.Rooms.Items.Floor.Wired.Effects;

internal abstract class WiredEffectRoomItem(IRoom room, int id, IUserInfo owner, Point3D position, int direction)
	: FloorRoomItem(room, id, owner, position, direction), IWiredEffectRoomItem
{
	public abstract override IWiredEffectFurniture Furniture { get; }

	public required int EffectDelay { get; set; }

	public override double Height => this.Furniture.DefaultHeight;

	public abstract void Interact(IUserRoomUnit unit, int state);
	public abstract void Trigger(IUserRoomUnit? cause = null);
}
