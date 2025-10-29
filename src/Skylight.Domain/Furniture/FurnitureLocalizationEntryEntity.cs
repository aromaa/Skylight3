namespace Skylight.Domain.Furniture;

public class FurnitureLocalizationEntryEntity
{
	public int LocalizationId { get; init; }
	public FurnitureLocalizationEntity? Localization { get; init; }

	public string Locale { get; init; } = null!;

	public string Name { get; set; } = null!;
	public string Description { get; set; } = null!;
}
