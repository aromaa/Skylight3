using Skylight.API.Game.Rooms;
using Skylight.API.Game.Rooms.Map;
using Skylight.API.Game.Rooms.Units;
using Skylight.API.Game.Users;
using Skylight.API.Numerics;
using Skylight.Protocol.Packets.Data.Room.Engine;
using Skylight.Protocol.Packets.Outgoing.Room.Engine;
using Skylight.Server.Game.Users;

namespace Skylight.Server.Game.Rooms.Units;

internal sealed class RoomUnit : IUserRoomUnit
{
	private readonly RoomUnitManager roomUnitManager;
	public IRoom Room { get; }

	public IUser User { get; }

	public int Id { get; }

	public bool InRoom { get; internal set; } = true;

	private Point3D position;
	public Point3D Position => this.position;

	private Point2D rotation;
	public Point2D Rotation => this.rotation;

	private Point3D nextStepPosition;
	public Point3D NextStepPosition => this.nextStepPosition;

	private Point2D targetLocation;
	public Point2D TargetLocation => this.targetLocation;

	public bool Moving => this.position.XY != this.nextStepPosition.XY;
	public bool Pathfinding => this.path.Count > 0;

	private Stack<Point2D> path;

	internal RoomUnit(RoomUnitManager roomUnitManager, Room room, User user, int id, Point3D position)
	{
		this.roomUnitManager = roomUnitManager;

		this.Room = room;

		this.User = user;

		this.Id = id;

		this.SetPositionInternal(position);

		this.path = new Stack<Point2D>();
	}

	public void Tick()
	{
		if (this.Moving)
		{
			this.position = this.nextStepPosition;
		}

		if (this.Pathfinding)
		{
			Point2D to = this.path.Pop();

			IRoomTile lastTile = this.Room.Map.GetTile(this.position.XY);
			IRoomTile nextTile = this.Room.Map.GetTile(to);

			this.nextStepPosition.XY = to;
			this.nextStepPosition.Z = nextTile.GetStepHeight(this.position.Z);

			lastTile.WalkOff(this);
			nextTile.WalkOn(this);

			int calculatedRotation = CalculateDirection(this.position.XY, this.nextStepPosition.XY);
			this.rotation = new Point2D(calculatedRotation, calculatedRotation);
		}
	}

	internal void SetPosition(Point3D position)
	{
		IRoomTile nextTile = this.Room.Map.GetTile(this.Moving ? this.nextStepPosition.XY : this.position.XY);
		nextTile.WalkOff(this);

		this.SetPositionInternal(position);
	}

	private void SetPositionInternal(Point3D position)
	{
		this.position = position;
		this.nextStepPosition = position;
		this.targetLocation = position.XY;

		IRoomTile nextTile = this.Room.Map.GetTile(this.position.XY);
		nextTile.WalkOn(this);
	}

	public void PathfindTo(Point2D target)
	{
		this.targetLocation = target;

		Point2D start = this.Moving ? this.nextStepPosition.XY : this.position.XY;

		this.path = this.Room.Map.PathfindTo(start, target, this);

		((RoomUnitManager)this.Room.UnitManager).Move(this);
	}

	public static int CalculateDirection(Point2D from, Point2D target)
	{
		if (from.X > target.X && from.Y > target.Y)
		{
			return 7;
		}
		else if (from.X < target.X && from.Y < target.Y)
		{
			return 3;
		}
		else if (from.X > target.X && from.Y < target.Y)
		{
			return 5;
		}
		else if (from.X < target.X && from.Y > target.Y)
		{
			return 1;
		}
		else if (from.X > target.X)
		{
			return 6;
		}
		else if (from.X < target.X)
		{
			return 2;
		}
		else if (from.Y < target.Y)
		{
			return 4;
		}
		else
		{
			return 0;
		}
	}

	public void LookTo(Point2D target)
	{
		if (this.Pathfinding || this.Moving || target == this.Position.XY)
		{
			return;
		}

		int newRotation = CalculateDirection(this.Position.XY, target);

		if (this.rotation.X != newRotation)
		{
			this.rotation = new Point2D(newRotation, newRotation);

			this.Room.SendAsync(new UserUpdateOutgoingPacket(new List<RoomUnitUpdateData>
			{
				new(this.Id, this.Position.X, this.Position.Y, this.Position.Z, this.Rotation.X, this.Rotation.Y, string.Empty)
			}));
		}
	}
}
