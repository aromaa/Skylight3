namespace Skylight.Domain.Catalog;

public class CatalogOfferLocalizationEntity
{
	public int Id { get; init; }

	public string Code { get; set; } = null!;

	public List<CatalogOfferLocalizationEntryEntity>? Entries { get; set; }
}
