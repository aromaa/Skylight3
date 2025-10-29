namespace Skylight.Domain.Catalog;

public class CatalogOfferEntity
{
	public int Id { get; init; }

	public int LocalizationId { get; set; }
	public CatalogOfferLocalizationEntity? Localization { get; set; }

	public List<CatalogProductEntity>? Products { get; set; }
}
