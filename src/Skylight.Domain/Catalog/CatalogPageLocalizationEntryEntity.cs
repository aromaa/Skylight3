namespace Skylight.Domain.Catalog;

public class CatalogPageLocalizationEntryEntity
{
	public int LocalizationId { get; init; }
	public CatalogPageLocalizationEntity? Localization { get; init; }

	public string Locale { get; init; } = null!;

	public string Name { get; set; } = null!;

	public List<string> Texts { get; set; } = null!;
	public List<string> Images { get; set; } = null!;
}
