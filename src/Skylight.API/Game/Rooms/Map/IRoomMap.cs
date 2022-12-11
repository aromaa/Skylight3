using Skylight.API.Game.Rooms.Units;
using Skylight.API.Numerics;

namespace Skylight.API.Game.Rooms.Map;

public interface IRoomMap
{
	public IRoomLayout Layout { get; }

	public bool IsValidLocation(Point2D location);

	public IRoomTile GetTile(Point2D location);

	public Stack<Point2D> PathfindTo(Point2D start, Point2D target, IRoomUnit unit);
}
