using System.Text.Json;

namespace Skylight.API.Game.Furniture;

public interface IFurnitureItemData
{
	public JsonDocument? GetExtraData();
}
