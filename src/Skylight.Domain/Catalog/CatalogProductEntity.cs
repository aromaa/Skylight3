namespace Skylight.Domain.Catalog;

public abstract class CatalogProductEntity
{
	public int Id { get; init; }

	public int OfferId { get; set; }
	public CatalogOfferEntity? Offer { get; set; }

	public int Amount { get; set; }

	public string ExtraData { get; set; } = null!;
}
