using Skylight.API.Game.Furniture.Floor.Wired.Effects;
using Skylight.API.Game.Rooms.Items;
using Skylight.API.Game.Rooms.Items.Floor.Wired.Effects;
using Skylight.API.Game.Rooms.Private;
using Skylight.API.Game.Rooms.Units;
using Skylight.API.Game.Users;
using Skylight.API.Numerics;

namespace Skylight.Server.Game.Rooms.Items.Floor.Wired.Effects;

internal abstract class WiredEffectRoomItem<T>(IPrivateRoom room, RoomItemId id, IUserInfo owner, T furniture, Point3D position, int direction, int effectDelay)
	: FloorRoomItem<T>(room, id, owner, furniture, position, direction), IWiredEffectRoomItem
	where T : IWiredEffectFurniture
{
	public int EffectDelay { get; set; } = effectDelay;

	public new IWiredEffectFurniture Furniture => this.furniture;

	public bool Interact(IUserRoomUnit unit, int state)
	{
		if (!this.Room.IsOwner(unit.User))
		{
			return false;
		}

		this.Open(unit);

		return true;
	}

	public abstract void Open(IUserRoomUnit unit);

	public abstract void Trigger(IUserRoomUnit? cause = null);
}
