using Skylight.API.Game.Furniture.Wall;
using Skylight.API.Game.Rooms.Items;
using Skylight.API.Game.Rooms.Items.Wall;
using Skylight.API.Game.Rooms.Private;
using Skylight.API.Game.Users;
using Skylight.API.Numerics;

namespace Skylight.Server.Game.Rooms.Items.Wall;

internal abstract class WallRoomItem<T>(IPrivateRoom room, RoomItemId id, IUserInfo owner, T furniture, Point2D location, Point2D position, int direction) : RoomItem<T>(room, id, owner, furniture), IWallRoomItem
	where T : IWallFurniture
{
	public Point2D Location { get; set; } = location;
	public Point2D Position { get; set; } = position;

	public int Direction { get; set; } = direction;

	public override int StripId => -this.Id.Id;

	public new IWallFurniture Furniture => this.furniture;
}
