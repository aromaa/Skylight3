using Skylight.API.Numerics;

namespace Skylight.API.Game.Rooms.Units;

public interface IRoomUnit
{
	public IRoom Room { get; }

	public int Id { get; }

	public bool InRoom { get; }

	public Point3D Position { get; }
	int BodyRotation { get; }
	int HeadRotation { get; }

	public Point3D NextStepPosition { get; }

	public Point2D TargetLocation { get; }

	public bool Moving => this.NextStepPosition.XY != this.Position.XY;
	public bool Pathfinding => this.TargetLocation != this.Position.XY;

	public void PathfindTo(Point2D target);

	public void Tick();
}
