using Skylight.API.Game.Furniture.Floor;
using Skylight.API.Game.Rooms.Items.Floor;
using Skylight.API.Game.Rooms.Private;
using Skylight.API.Game.Rooms.Units;
using Skylight.API.Game.Users;
using Skylight.API.Numerics;

namespace Skylight.Server.Game.Rooms.Items.Floor;

internal sealed class VariableHeightRoomItem(IPrivateRoom room, int id, IUserInfo owner, IVariableHeightFurniture furniture, Point3D position, int direction, int state)
	: MultiStateFloorRoomItem<IVariableHeightFurniture>(room, id, owner, furniture, position, direction, state), IVariableHeightRoomItem
{
	public new IVariableHeightFurniture Furniture => this.furniture;

	public override double Height => this.furniture.Heights[this.State];

	public bool Interact(IUserRoomUnit unit, int state) => this.CycleState(unit);
}
