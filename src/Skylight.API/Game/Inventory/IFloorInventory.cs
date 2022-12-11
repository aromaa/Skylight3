using System.Diagnostics.CodeAnalysis;
using Skylight.API.Game.Inventory.Items.Floor;

namespace Skylight.API.Game.Inventory;

public interface IFloorInventory : IFurnitureInventory
{
	public IEnumerable<IFloorInventoryItem> FloorItems { get; }

	public bool TryAddFloorItem(IFloorInventoryItem item);
	public bool TryRemoveFloorItem(IFloorInventoryItem item);

	public void AddUnseenFloorItem(IFloorInventoryItem item);

	public bool TryGetFloorItem(int itemId, [NotNullWhen(true)] out IFloorInventoryItem? item);
}
