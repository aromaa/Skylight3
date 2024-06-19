namespace Skylight.Server.Game.Furniture.Floor;

internal abstract class FixedHeightStatefulFloorFurniture(int id, int width, int length, double height) : StatefulFloorFurniture(id, width, length)
{
	public override double DefaultHeight => height;
}
