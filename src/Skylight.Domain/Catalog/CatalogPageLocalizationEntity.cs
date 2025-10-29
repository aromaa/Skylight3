namespace Skylight.Domain.Catalog;

public class CatalogPageLocalizationEntity
{
	public int Id { get; init; }

	public string Code { get; set; } = null!;

	public List<CatalogPageLocalizationEntryEntity>? Entries { get; set; }
}
