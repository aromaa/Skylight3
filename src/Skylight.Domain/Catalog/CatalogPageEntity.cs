namespace Skylight.Domain.Catalog;

public class CatalogPageEntity
{
	public int Id { get; init; }

	public int LocalizationId { get; set; }
	public CatalogPageLocalizationEntity? Localization { get; set; }

	public int IconColor { get; set; }
	public int IconImage { get; set; }

	public string Layout { get; set; } = null!;
}
