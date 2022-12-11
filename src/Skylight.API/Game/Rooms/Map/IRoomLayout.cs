using Skylight.API.Numerics;

namespace Skylight.API.Game.Rooms.Map;

public interface IRoomLayout
{
	public string Id { get; }

	public Point2D Size { get; }

	public string HeightMap { get; }

	public Point2D DoorLocation { get; }
	public int DoorDirection { get; }
}
