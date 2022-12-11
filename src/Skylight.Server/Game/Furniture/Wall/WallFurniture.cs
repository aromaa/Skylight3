using Skylight.API.Game.Furniture.Wall;

namespace Skylight.Server.Game.Furniture.Wall;

internal abstract class WallFurniture : AbstractFurniture, IWallFurniture
{
	internal WallFurniture(int id)
		: base(id)
	{
	}
}
