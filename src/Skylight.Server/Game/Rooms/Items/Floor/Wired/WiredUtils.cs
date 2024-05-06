using System.Text.Json;

namespace Skylight.Server.Game.Rooms.Items.Floor.Wired;

internal static class WiredUtils
{
	internal static HashSet<int> GetSelectedItems(JsonDocument? extraData)
	{
		if (extraData is null || !extraData.RootElement.TryGetProperty("SelectedItems", out JsonElement selectedItemsValue))
		{
			return [];
		}

		HashSet<int> stripIds = [];
		foreach (JsonElement selectedItemValue in selectedItemsValue.EnumerateArray())
		{
			stripIds.Add(selectedItemValue.GetInt32());
		}

		return stripIds;
	}
}
