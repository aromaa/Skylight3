using Skylight.API.Game.Furniture.Floor;
using Skylight.API.Game.Rooms;
using Skylight.API.Game.Users;
using Skylight.API.Numerics;

namespace Skylight.Server.Game.Rooms.Items.Floor;

internal abstract class FixedHeightStatefulFloorRoomItem<T>(IRoom room, int id, IUserInfo owner, T furniture, Point3D position, int direction)
	: StatefulFloorRoomItem<T>(room, id, owner, furniture, position, direction)
	where T : IStatefulFloorFurniture
{
	public override double Height => this.Furniture.DefaultHeight;
}
