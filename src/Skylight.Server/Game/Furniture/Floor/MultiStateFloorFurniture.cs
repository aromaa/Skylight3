using Skylight.API.Game.Furniture.Floor;
using Skylight.API.Numerics;

namespace Skylight.Server.Game.Furniture.Floor;

internal abstract class MultiStateFloorFurniture(int id, FloorFurnitureKind kind, Point2D dimensions, int stateCount) : StatefulFloorFurniture(id, kind, dimensions), IMultiStateFloorFurniture
{
	public int StateCount => stateCount;
}
