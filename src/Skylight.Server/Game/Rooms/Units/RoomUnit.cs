using Skylight.API.Game.Rooms;
using Skylight.API.Game.Rooms.Items.Interactions.Wired.Triggers;
using Skylight.API.Game.Rooms.Map;
using Skylight.API.Game.Rooms.Units;
using Skylight.API.Game.Users;
using Skylight.API.Numerics;
using Skylight.Protocol.Packets.Data.Room.Engine;
using Skylight.Protocol.Packets.Outgoing.Room.Chat;
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

	public Point3D Position { get; private set; }
	public Point2D Rotation { get; private set; }

	public Point3D NextStepPosition { get; private set; }
	public Point2D TargetLocation { get; private set; }

	public bool Moving => this.Position.XY != this.NextStepPosition.XY;
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
			this.Position = this.NextStepPosition;
		}

		if (this.Pathfinding)
		{
			Point2D to = this.path.Pop();

			IRoomTile lastTile = this.Room.Map.GetTile(this.Position.XY);
			IRoomTile nextTile = this.Room.Map.GetTile(to);

			this.NextStepPosition = new Point3D(to, nextTile.GetStepHeight(this.Position.Z));

			lastTile.WalkOff(this);
			nextTile.WalkOn(this);

			int calculatedRotation = CalculateDirection(this.Position.XY, this.NextStepPosition.XY);
			this.Rotation = new Point2D(calculatedRotation, calculatedRotation);
		}
	}

	internal void SetPosition(Point3D position)
	{
		IRoomTile nextTile = this.Room.Map.GetTile(this.Moving ? this.NextStepPosition.XY : this.Position.XY);
		nextTile.WalkOff(this);

		this.SetPositionInternal(position);
	}

	private void SetPositionInternal(Point3D position)
	{
		this.Position = position;
		this.NextStepPosition = position;
		this.TargetLocation = position.XY;

		IRoomTile nextTile = this.Room.Map.GetTile(this.Position.XY);
		nextTile.WalkOn(this);
	}

	public void PathfindTo(Point2D target)
	{
		this.TargetLocation = target;

		Point3D start = this.Moving ? this.NextStepPosition : this.Position;

		this.path = this.Room.Map.PathfindTo(start, new Point3D(target, double.NaN), this);

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

		if (this.Rotation.X != newRotation)
		{
			this.Rotation = new Point2D(newRotation, newRotation);

			this.Room.SendAsync(new UserUpdateOutgoingPacket(
			[
				new RoomUnitUpdateData(this.Id, this.Position.X, this.Position.Y, this.Position.Z, this.Rotation.X, this.Rotation.Y, string.Empty)
			]));
		}
	}

	public void Chat(string message, int styleId = 0, int trackingId = -1)
	{
		if (this.TriggerOnSayWired(message, styleId, trackingId))
		{
			return;
		}

		this.Room.SendAsync(new ChatOutgoingPacket(this.Id, message, 0, styleId, trackingId, Array.Empty<(string, string, bool)>()));
	}

	public void Shout(string message, int styleId = 0, int trackingId = -1)
	{
		if (this.TriggerOnSayWired(message, styleId, trackingId))
		{
			return;
		}

		this.Room.SendAsync(new ShoutOutgoingPacket(this.Id, message, 0, styleId, trackingId, Array.Empty<(string, string, bool)>()));
	}

	private bool TriggerOnSayWired(string message, int styleId = 0, int trackingId = -1)
	{
		if (!this.Room.ItemManager.TryGetInteractionHandler(out IUserSayTriggerInteractionHandler? interactionHandler) || !interactionHandler.OnSay(this, message))
		{
			return false;
		}

		this.User.SendAsync(new WhisperOutgoingPacket(this.Id, message, 0, styleId, trackingId, Array.Empty<(string, string, bool)>()));

		return true;
	}
}
