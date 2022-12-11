using System.Diagnostics.CodeAnalysis;
using Skylight.API.Game.Furniture;
using Skylight.API.Game.Furniture.Floor;
using Skylight.API.Game.Furniture.Wall;

namespace Skylight.Server.Game.Furniture;

internal partial class FurnitureManager
{
	public bool TryGetFloorFurniture(int furnitureId, [NotNullWhen(true)] out IFloorFurniture? furniture) => this.Current.TryGetFloorFurniture(furnitureId, out furniture);
	public bool TryGetWallFurniture(int furnitureId, [NotNullWhen(true)] out IWallFurniture? furniture) => this.Current.TryGetWallFurniture(furnitureId, out furniture);

	private sealed class Snapshot : IFurnitureSnapshot
	{
		private readonly Cache cache;

		internal Snapshot(Cache cache)
		{
			this.cache = cache;
		}

		public bool TryGetFloorFurniture(int furnitureId, [NotNullWhen(true)] out IFloorFurniture? furniture) => this.cache.FloorFurnitures.TryGetValue(furnitureId, out furniture);
		public bool TryGetWallFurniture(int furnitureId, [NotNullWhen(true)] out IWallFurniture? furniture) => this.cache.WallFurnitures.TryGetValue(furnitureId, out furniture);
	}
}
