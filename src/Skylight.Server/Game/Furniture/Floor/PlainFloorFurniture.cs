using Skylight.API.Game.Furniture.Floor;
using Skylight.API.Numerics;

namespace Skylight.Server.Game.Furniture.Floor;

internal abstract class PlainFloorFurniture(int id, FloorFurnitureType type, Point2D dimensions, double height) : FloorFurniture(id, type, dimensions), IPlainFloorFurniture
{
	public override double DefaultHeight => height;
}
