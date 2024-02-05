using Skylight.API.Game.Furniture.Floor;
using Skylight.API.Game.Rooms;
using Skylight.API.Game.Rooms.Items.Floor;
using Skylight.API.Game.Rooms.Units;
using Skylight.API.Game.Users;
using Skylight.API.Numerics;

namespace Skylight.Server.Game.Rooms.Items.Floor;

internal class MultiStateFloorRoomItem<T> : FloorRoomItem, IMultiStateFloorItem
	where T : IMultiStateFloorFurniture
{
	private readonly T furniture;

	public int State { get; private set; }

	public MultiStateFloorRoomItem(IRoom room, int id, IUserInfo owner, T furniture, Point3D position, int direction)
		: base(room, id, owner, position, direction)
	{
		this.furniture = furniture;
	}

	public override double Height => this.Furniture.DefaultHeight;

	public override IMultiStateFloorFurniture Furniture => this.furniture;

	public void Interact(IUserRoomUnit unit, int state)
	{
		this.State = (this.State + 1) % this.furniture.StateCount;

		this.Room.ItemManager.UpdateItem(this);
	}
}
