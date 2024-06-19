using Skylight.API.Game.Furniture.Floor;

namespace Skylight.Server.Game.Furniture.Floor;

internal abstract class MultiStateFloorFurniture(int id, int width, int length, int stateCount) : StatefulFloorFurniture(id, width, length), IMultiStateFloorFurniture
{
	public int StateCount => stateCount;
}
