using Skylight.API.Game.Furniture.Floor;
using Skylight.API.Game.Rooms;
using Skylight.API.Game.Rooms.Items.Floor;
using Skylight.API.Game.Rooms.Map;
using Skylight.API.Game.Users;
using Skylight.API.Numerics;

namespace Skylight.Server.Game.Rooms.Items.Floor;

internal abstract class FloorRoomItem : IFloorRoomItem
{
	public IRoom Room { get; }

	public int Id { get; }

	public IUserInfo Owner { get; }

	public abstract IFloorFurniture Furniture { get; }

	public Point3D Position { get; internal set; }

	public int Direction { get; internal set; }

	public abstract double Height { get; }

	internal FloorRoomItem(IRoom room, int id, IUserInfo owner, Point3D position, int direction)
	{
		this.Room = room;

		this.Id = id;

		this.Owner = owner;

		this.Position = position;
		this.Direction = direction;
	}

	public int StripId => this.Id;
	public EffectiveTilesEnumerator EffectiveTiles => new(this.Furniture.EffectiveTiles, this.Direction);

	public virtual void OnPlace()
	{
	}

	public virtual void OnMove(Point3D position, int direction)
	{
		this.Position = position;
		this.Direction = direction;
	}

	public virtual void OnRemove()
	{
	}
}
