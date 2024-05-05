using System.Text.Json;

namespace Skylight.Domain.Items;

public class WallItemDataEntity
{
	public int WallItemId { get; init; }
	public WallItemEntity? WallItem { get; set; }

	public JsonDocument? ExtraData { get; set; }
}
