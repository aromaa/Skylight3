using Skylight.API.Game.Furniture.Floor.Wired.Effects;
using Skylight.API.Game.Rooms;
using Skylight.API.Game.Rooms.Items.Floor.Wired.Effects;
using Skylight.API.Game.Rooms.Units;
using Skylight.API.Game.Users;
using Skylight.API.Numerics;

namespace Skylight.Server.Game.Rooms.Items.Floor.Wired.Effects;

internal abstract class WiredEffectRoomItem<T>(IRoom room, int id, IUserInfo owner, T furniture, Point3D position, int direction, int effectDelay)
	: FloorRoomItem<T>(room, id, owner, furniture, position, direction), IWiredEffectRoomItem
	where T : IWiredEffectFurniture
{
	public int EffectDelay { get; set; } = effectDelay;

	public new IWiredEffectFurniture Furniture => this.furniture;

	public override double Height => this.Furniture.DefaultHeight;

	public abstract void Interact(IUserRoomUnit unit, int state);
	public abstract void Trigger(IUserRoomUnit? cause = null);
}
