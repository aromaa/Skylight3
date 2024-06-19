using Skylight.API.Game.Furniture.Floor;

namespace Skylight.Server.Game.Furniture.Floor;

internal abstract class PlainFloorFurniture(int id, int width, int length, double height) : FloorFurniture(id, width, length), IPlainFloorFurniture
{
	public override double DefaultHeight => height;
}
