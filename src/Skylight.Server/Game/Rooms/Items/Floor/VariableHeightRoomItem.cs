using Skylight.API.Game.Furniture.Floor;
using Skylight.API.Game.Rooms;
using Skylight.API.Game.Rooms.Items.Floor;
using Skylight.API.Game.Users;
using Skylight.API.Numerics;

namespace Skylight.Server.Game.Rooms.Items.Floor;

internal sealed class VariableHeightRoomItem(IRoom room, int id, IUserInfo owner, IVariableHeightFurniture furniture, Point3D position, int direction, int state)
	: MultiStateFloorRoomItem<IVariableHeightFurniture>(room, id, owner, furniture, position, direction, state), IVariableHeightRoomItem
{
	public new IVariableHeightFurniture Furniture => this.furniture;

	public override double Height => this.furniture.Heights[this.State];
}
