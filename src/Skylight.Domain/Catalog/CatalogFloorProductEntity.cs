using Skylight.Domain.Furniture;

namespace Skylight.Domain.Catalog;

public class CatalogFloorProductEntity : CatalogProductEntity
{
	public int FurnitureId { get; set; }
	public FloorFurnitureEntity? Furniture { get; set; }
}
