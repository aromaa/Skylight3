using Skylight.API.Game.Furniture.Floor;
using Skylight.API.Numerics;

namespace Skylight.Server.Game.Furniture.Floor;

internal abstract class FixedHeightMultiStateFloorFurniture(int id, IFloorFurnitureKind kind, Point2D dimensions, double height, int stateCount) : MultiStateFloorFurniture(id, kind, dimensions, stateCount)
{
	public override double DefaultHeight => height;
}
