using Skylight.Domain.Furniture;

namespace Skylight.Domain.Catalog;

public class CatalogFloorProductEntity : CatalogProductEntity
{
	public int FurnitureId { get; set; }
	public FloorFurnitureEntity? Furniture { get; set; }

	public int Amount { get; set; }

	public string ExtraData { get; set; } = null!;
}
