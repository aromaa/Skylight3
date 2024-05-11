using Skylight.API.Game.Furniture;
using Skylight.API.Game.Furniture.Wall;

namespace Skylight.API.Game.Inventory.Items.Wall;

public interface IStickyNoteInventoryItem : IWallInventoryItem, IFurnitureItemData, IFurnitureItem<IStickyNoteFurniture>
{
	public new IStickyNoteFurniture Furniture { get; }

	public int Count { get; }

	public Task<IStickyNoteInventoryItem?> TryConsumeAsync(int roomId, CancellationToken cancellationToken = default);

	IWallFurniture IWallInventoryItem.Furniture => this.Furniture;
}
