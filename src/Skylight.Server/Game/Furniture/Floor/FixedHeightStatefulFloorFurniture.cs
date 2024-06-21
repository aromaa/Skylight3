using Skylight.API.Game.Furniture.Floor;
using Skylight.API.Numerics;

namespace Skylight.Server.Game.Furniture.Floor;

internal abstract class FixedHeightStatefulFloorFurniture(int id, FloorFurnitureType type, Point2D dimensions, double height) : StatefulFloorFurniture(id, type, dimensions)
{
	public override double DefaultHeight => height;
}
