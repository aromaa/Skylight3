using Skylight.API.Game.Furniture.Floor;
using Skylight.API.Numerics;

namespace Skylight.Server.Game.Furniture.Floor;

internal abstract class FixedHeightMultiStateFloorFurniture(int id, FloorFurnitureType type, Point2D dimensions, double height, int stateCount) : MultiStateFloorFurniture(id, type, dimensions, stateCount)
{
	public override double DefaultHeight => height;
}
