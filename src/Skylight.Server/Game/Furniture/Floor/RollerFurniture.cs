using Skylight.API.Game.Furniture.Floor;

namespace Skylight.Server.Game.Furniture.Floor;

internal sealed class RollerFurniture : FloorFurniture, IRollerFurniture
{
	public RollerFurniture(int id, int width, int length, double height)
		: base(id, width, length, height)
	{
	}
}
