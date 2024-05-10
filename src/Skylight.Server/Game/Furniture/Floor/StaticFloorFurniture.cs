using Skylight.API.Game.Furniture.Floor;

namespace Skylight.Server.Game.Furniture.Floor;

internal sealed class StaticFloorFurniture(int id, int width, int length, double height) : FloorFurniture(id, width, length), IStaticFloorFurniture
{
	public override double DefaultHeight => height;
}
