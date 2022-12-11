using System.Text.Json;

namespace Skylight.Domain.Furniture;

public class WallFurnitureEntity
{
	public int Id { get; init; }

	public string InteractionType { get; set; } = null!;
	public JsonDocument? InteractionData { get; set; }
}
