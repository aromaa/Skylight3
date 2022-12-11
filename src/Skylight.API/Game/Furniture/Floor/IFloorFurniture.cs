using System.Collections.Immutable;
using Skylight.API.Numerics;

namespace Skylight.API.Game.Furniture.Floor;

public interface IFloorFurniture : IFurniture
{
	public double Height { get; }

	public ImmutableArray<Point2D> EffectiveTiles { get; }
}
