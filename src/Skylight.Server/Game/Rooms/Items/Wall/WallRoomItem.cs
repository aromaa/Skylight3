using Skylight.API.Game.Furniture.Wall;
using Skylight.API.Game.Rooms;
using Skylight.API.Game.Rooms.Items.Wall;
using Skylight.API.Game.Users;
using Skylight.API.Numerics;

namespace Skylight.Server.Game.Rooms.Items.Wall;

internal abstract class WallRoomItem : IWallRoomItem
{
	public IRoom Room { get; }

	public int Id { get; }

	public IUserInfo Owner { get; }

	public abstract IWallFurniture Furniture { get; }

	public Point2D Location { get; set; }
	public Point2D Position { get; set; }

	public int Direction { get; set; }

	internal WallRoomItem(IRoom room, int id, IUserInfo owner, Point2D location, Point2D position, int direction)
	{
		this.Room = room;

		this.Id = id;

		this.Owner = owner;

		this.Location = location;
		this.Position = position;
		this.Direction = direction;
	}

	public int StripId => -this.Id;

	public virtual void OnPlace()
	{
	}

	public virtual void OnRemove()
	{
	}
}
