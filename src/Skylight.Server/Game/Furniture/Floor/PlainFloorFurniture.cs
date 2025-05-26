using Skylight.API.Game.Furniture.Floor;
using Skylight.API.Numerics;

namespace Skylight.Server.Game.Furniture.Floor;

internal abstract class PlainFloorFurniture(int id, IFloorFurnitureKind kind, Point2D dimensions, double height) : FloorFurniture(id, kind, dimensions), IPlainFloorFurniture
{
	public override double DefaultHeight => height;
}
