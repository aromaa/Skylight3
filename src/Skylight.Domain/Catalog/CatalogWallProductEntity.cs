using Skylight.Domain.Furniture;

namespace Skylight.Domain.Catalog;

public class CatalogWallProductEntity : CatalogProductEntity
{
	public int FurnitureId { get; set; }
	public WallFurnitureEntity? Furniture { get; set; }

	public int Amount { get; set; }

	public string ExtraData { get; set; } = null!;
}
