namespace Skylight.Domain.Catalog;

public class CatalogOfferCostEntity
{
	public int Id { get; init; }

	public int OfferId { get; set; }
	public CatalogOfferEntity? Offer { get; set; }

	public string CurrencyType { get; set; } = null!;
	public string? CurrencyData { get; set; }

	public int Amount { get; set; }
}
