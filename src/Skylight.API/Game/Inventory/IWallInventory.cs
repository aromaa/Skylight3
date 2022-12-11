using System.Diagnostics.CodeAnalysis;
using Skylight.API.Game.Inventory.Items.Wall;

namespace Skylight.API.Game.Inventory;

public interface IWallInventory
{
	public IEnumerable<IWallInventoryItem> WallItems { get; }

	public bool TryAddWallItem(IWallInventoryItem item);
	public bool TryRemoveWallItem(IWallInventoryItem item);

	public bool TryGetWallItem(int itemId, [NotNullWhen(true)] out IWallInventoryItem? item);
}
