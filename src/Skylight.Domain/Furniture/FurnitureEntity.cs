using System.Text.Json;

namespace Skylight.Domain.Furniture;

public abstract class FurnitureEntity
{
	public int Id { get; init; }

	public int Revision { get; set; }

	public string ClassName { get; set; } = null!;

	public string InteractionType { get; set; } = null!;
	public JsonDocument? InteractionData { get; set; }

	public int? LocalizationId { get; set; }
	public FurnitureLocalizationEntity? Localization { get; set; }
}
