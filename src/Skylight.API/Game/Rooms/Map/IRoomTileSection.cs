using Skylight.API.Game.Rooms.Units;
using Skylight.API.Numerics;

namespace Skylight.API.Game.Rooms.Map;

public interface IRoomTileSection
{
	public Point3D Position { get; }

	public void WalkOn(IRoomUnit unit);
	public void WalkOff(IRoomUnit unit);
}
