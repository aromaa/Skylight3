using Skylight.Domain.Furniture;

namespace Skylight.Domain.Catalog;

public class CatalogProductEntity
{
	public int Id { get; init; }

	public int OfferId { get; set; }
	public CatalogOfferEntity? Offer { get; set; }

	public int? FloorFurnitureId { get; set; }
	public FloorFurnitureEntity? FloorFurniture { get; set; }

	public int? WallFurnitureId { get; set; }
	public WallFurnitureEntity? WallFurniture { get; set; }

	public int Amount { get; set; }

	public string ExtraData { get; set; } = null!;
}
