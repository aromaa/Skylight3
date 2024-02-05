using System.Collections.Immutable;
using Skylight.API.Game.Rooms.Map;
using Skylight.API.Numerics;

namespace Skylight.API.Game.Furniture.Floor;

public interface IFloorFurniture : IFurniture
{
	public ImmutableArray<Point2D> EffectiveTiles { get; }

	public double DefaultHeight { get; }

	public EffectiveTilesEnumerator GetEffectiveTiles(int direction);
}
