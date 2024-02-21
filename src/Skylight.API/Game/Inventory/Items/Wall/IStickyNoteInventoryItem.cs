using Skylight.API.Game.Furniture;
using Skylight.API.Game.Furniture.Wall;

namespace Skylight.API.Game.Inventory.Items.Wall;

public interface IStickyNoteInventoryItem : IWallInventoryItem<IStickyNoteFurniture>, IFurnitureItemData
{
	public int Count { get; }

	public Task<IStickyNoteInventoryItem?> TryConsumeAsync(int roomId, CancellationToken cancellationToken = default);
}
