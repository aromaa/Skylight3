namespace Skylight.Domain.Catalog;

public abstract class CatalogProductEntity
{
	public int Id { get; init; }

	public int OfferId { get; set; }
	public CatalogOfferEntity? Offer { get; set; }
}
