namespace Skylight.Domain.Catalog;

public class CatalogOfferLocalizationEntryEntity
{
	public int LocalizationId { get; init; }
	public CatalogOfferLocalizationEntity? Localization { get; init; }

	public string Locale { get; init; } = null!;

	public string Name { get; set; } = null!;
	public string Description { get; set; } = null!;
}
