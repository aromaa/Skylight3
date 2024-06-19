namespace Skylight.Server.Game.Furniture.Floor;

internal abstract class FixedHeightMultiStateFloorFurniture(int id, int width, int length, double height, int stateCount) : MultiStateFloorFurniture(id, width, length, stateCount)
{
	public override double DefaultHeight => height;
}
