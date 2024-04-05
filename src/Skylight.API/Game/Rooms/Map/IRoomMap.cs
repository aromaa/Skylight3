using Skylight.API.Game.Rooms.Units;
using Skylight.API.Numerics;

namespace Skylight.API.Game.Rooms.Map;

public interface IRoomMap
{
	public IRoomLayout Layout { get; }

	public bool IsValidLocation(Point2D location);

	public IRoomTile GetTile(int x, int y);
	public IRoomTile GetTile(Point2D location);

	public Stack<Point2D> PathfindTo(Point3D start, Point3D target, IRoomUnit unit);
}
