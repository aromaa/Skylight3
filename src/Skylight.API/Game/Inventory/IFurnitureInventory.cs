using System.Diagnostics.CodeAnalysis;
using Skylight.API.Game.Inventory.Items;

namespace Skylight.API.Game.Inventory;

public interface IFurnitureInventory
{
	public void RefreshFurniture();

	public bool TryRemoveFurniture(IFurnitureInventoryItem item);
	public bool TryGetFurnitureItem(int stripId, [NotNullWhen(true)] out IFurnitureInventoryItem? item);
}
