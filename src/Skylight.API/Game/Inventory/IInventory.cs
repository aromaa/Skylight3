using Skylight.API.Game.Inventory.Items;

namespace Skylight.API.Game.Inventory;

public interface IInventory : IFloorInventory, IWallInventory, IBadgeInventory
{
	public void AddUnseenItems(IEnumerable<IInventoryItem> items);
}
