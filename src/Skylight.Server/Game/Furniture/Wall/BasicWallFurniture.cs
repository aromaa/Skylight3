using Skylight.API.Game.Furniture.Wall;

namespace Skylight.Server.Game.Furniture.Wall;

internal sealed class BasicWallFurniture : WallFurniture, IBasicWallFurniture
{
	internal BasicWallFurniture(int id)
		: base(id)
	{
	}
}
