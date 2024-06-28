using Skylight.API.Game.Furniture.Floor;
using Skylight.API.Game.Rooms;
using Skylight.API.Game.Rooms.Items;
using Skylight.API.Game.Rooms.Items.Floor;
using Skylight.API.Game.Rooms.Units;
using Skylight.API.Game.Users;
using Skylight.API.Numerics;

namespace Skylight.Server.Game.Rooms.Items.Floor;

internal abstract class MultiStateFloorRoomItem<T>(IRoom room, int id, IUserInfo owner, T furniture, Point3D position, int direction, int state)
	: StatefulFloorRoomItem<T>(room, id, owner, furniture, position, direction), IMultiStateFloorItem
	where T : IMultiStateFloorFurniture
{
	public new IMultiStateFloorFurniture Furniture => this.furniture;

	public int InternalState { get; set; } = state;

	public override int State => this.InternalState;

	int IMultiStateRoomItem.State
	{
		get => this.InternalState;
		set => this.InternalState = value;
	}

	protected bool CycleState(IUserRoomUnit unit)
	{
		if (!this.Room.IsOwner(unit.User))
		{
			return false;
		}

		this.InternalState = (this.State + 1) % this.furniture.StateCount;

		this.Room.ItemManager.UpdateItem(this);

		return true;
	}
}
