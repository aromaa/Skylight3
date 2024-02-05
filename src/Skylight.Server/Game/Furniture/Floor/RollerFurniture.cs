using Skylight.API.Game.Furniture.Floor;

namespace Skylight.Server.Game.Furniture.Floor;

internal sealed class RollerFurniture(int id, int width, int length, double height) : FloorFurniture(id, width, length), IRollerFurniture
{
	public override double DefaultHeight => height;
}
