using Skylight.API.Game.Furniture.Floor;
using Skylight.API.Game.Rooms.Items.Floor;
using Skylight.API.Game.Rooms.Private;
using Skylight.API.Game.Users;
using Skylight.API.Numerics;

namespace Skylight.Server.Game.Rooms.Items.Floor;

internal abstract class PlainFloorRoomItem<T>(IPrivateRoom room, int id, IUserInfo owner, T furniture, Point3D position, int direction)
	: FloorRoomItem<T>(room, id, owner, furniture, position, direction), IPlainFloorRoomItem
	where T : IPlainFloorFurniture
{
	public new IPlainFloorFurniture Furniture => this.furniture;

	public override double Height => this.Furniture.DefaultHeight;
}
