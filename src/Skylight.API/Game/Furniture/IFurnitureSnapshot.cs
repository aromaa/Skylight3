using System.Diagnostics.CodeAnalysis;
using Skylight.API.Game.Furniture.Floor;
using Skylight.API.Game.Furniture.Wall;

namespace Skylight.API.Game.Furniture;

public interface IFurnitureSnapshot
{
	public bool TryGetFloorFurniture(int furnitureId, [NotNullWhen(true)] out IFloorFurniture? furniture);
	public bool TryGetWallFurniture(int furnitureId, [NotNullWhen(true)] out IWallFurniture? furniture);
}
