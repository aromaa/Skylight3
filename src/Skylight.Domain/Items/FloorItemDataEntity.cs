using System.Text.Json;

namespace Skylight.Domain.Items;

public class FloorItemDataEntity
{
	public int FloorItemId { get; init; }
	public FloorItemEntity? FloorItem { get; set; }

	public JsonDocument? ExtraData { get; set; }
}
