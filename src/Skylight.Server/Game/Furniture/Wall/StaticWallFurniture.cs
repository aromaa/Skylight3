using Skylight.API.Game.Furniture.Wall;

namespace Skylight.Server.Game.Furniture.Wall;

internal sealed class StaticWallFurniture(int id) : WallFurniture(id), IStaticWallFurniture;
