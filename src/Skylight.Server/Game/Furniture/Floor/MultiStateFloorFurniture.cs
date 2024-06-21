using Skylight.API.Game.Furniture.Floor;
using Skylight.API.Numerics;

namespace Skylight.Server.Game.Furniture.Floor;

internal abstract class MultiStateFloorFurniture(int id, FloorFurnitureType type, Point2D dimensions, int stateCount) : StatefulFloorFurniture(id, type, dimensions), IMultiStateFloorFurniture
{
	public int StateCount => stateCount;
}
