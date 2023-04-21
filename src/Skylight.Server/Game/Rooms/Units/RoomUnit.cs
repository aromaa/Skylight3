using Skylight.API.Game.Rooms;
using Skylight.API.Game.Rooms.Map;
using Skylight.API.Game.Rooms.Units;
using Skylight.API.Game.Users;
using Skylight.API.Numerics;
using Skylight.Server.Game.Users;

namespace Skylight.Server.Game.Rooms.Units;

internal sealed class RoomUnit : IUserRoomUnit
{
	public IRoom Room { get; }

	public IUser User { get; }

	public int Id { get; }

	public bool InRoom { get; internal set; } = true;

	private Point3D position;
	public Point3D Position => this.position;

	private Point2D rotation;
	public int BodyRotation => this.rotation.X;
	public int HeadRotation { get; private set; }

	private Point3D nextStepPosition;
	public Point3D NextStepPosition => this.nextStepPosition;

	private Point2D targetLocation;
	public Point2D TargetLocation => this.targetLocation;

	public bool Moving => this.position.XY != this.nextStepPosition.XY;
	public bool Pathfinding => this.path.Count > 0;
	private readonly RoomUnitManager roomUnitManager;

	private Stack<Point2D> path;

	internal RoomUnit(Room room, User user, int id, Point3D position, RoomUnitManager roomUnitManager)
	{
		this.Room = room;

		this.User = user;

		this.Id = id;

		this.SetPositionInternal(position);

		this.path = new Stack<Point2D>();

		this.roomUnitManager = roomUnitManager;
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

			this.rotation = new Point2D(Walk(this.position.XY, this.nextStepPosition.XY), Walk(this.position.XY, this.nextStepPosition.XY));
			this.HeadRotation = this.rotation.X;
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

	public static int Walk(Point2D from, Point2D target)
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

	public async Task LookToAsync(int x, int y)
	{
		if (this.Pathfinding || this.Moving)
		{
			return;
		}

		Point2D target = new(x, y);

		if (target == this.Position.XY)
		{
			return;
		}

		Point2D newRotation = CalculateRotation(this.Position.XY, target);

		if (this.rotation != newRotation)
		{
			this.rotation = newRotation;
			this.HeadRotation = newRotation.X;

			await this.roomUnitManager.BroadcastRotationUpdateAsync(this).ConfigureAwait(false);
		}
	}

	private static Point2D CalculateRotation(Point2D currentPosition, Point2D targetPosition)
	{
		int rotation;

		if (currentPosition.X > targetPosition.X && currentPosition.Y > targetPosition.Y)
		{
			rotation = 7;
		}
		else if (currentPosition.X < targetPosition.X && currentPosition.Y < targetPosition.Y)
		{
			rotation = 3;
		}
		else if (currentPosition.X > targetPosition.X && currentPosition.Y < targetPosition.Y)
		{
			rotation = 5;
		}
		else if (currentPosition.X < targetPosition.X && currentPosition.Y > targetPosition.Y)
		{
			rotation = 1;
		}
		else if (currentPosition.X > targetPosition.X)
		{
			rotation = 6;
		}
		else if (currentPosition.X < targetPosition.X)
		{
			rotation = 2;
		}
		else if (currentPosition.Y < targetPosition.Y)
		{
			rotation = 4;
		}
		else
		{
			rotation = 0;
		}

		return new(rotation, rotation);
	}
}
