using Skylight.API.Game.Inventory.Items;

namespace Skylight.API.Game.Inventory;

public interface IInventory : IFloorInventory, IWallInventory
{
	public void AddUnseenItems(IEnumerable<IInventoryItem> items);
}
