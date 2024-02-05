using Skylight.API.Game.Furniture.Floor;

namespace Skylight.Server.Game.Furniture.Floor;

internal sealed class MultiStateFloorFurniture(int id, int width, int length, double height, int stateCount) : FloorFurniture(id, width, length), IMultiStateFloorFurniture
{
	public override double DefaultHeight => height;

	public int StateCount => stateCount;
}
