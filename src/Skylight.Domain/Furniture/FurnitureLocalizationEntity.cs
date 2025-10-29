namespace Skylight.Domain.Furniture;

public class FurnitureLocalizationEntity
{
	public int Id { get; init; }

	public List<FurnitureLocalizationEntryEntity>? Entries { get; set; }
}
