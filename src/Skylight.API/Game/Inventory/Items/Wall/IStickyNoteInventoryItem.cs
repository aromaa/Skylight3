using Skylight.API.Game.Furniture;
using Skylight.API.Game.Furniture.Wall;

namespace Skylight.API.Game.Inventory.Items.Wall;

public interface IStickyNoteInventoryItem : IWallInventoryItem, IFurnitureItem<IStickyNoteFurniture>, IFurnitureData<int>
{
	public new IStickyNoteFurniture Furniture { get; }

	public int Count { get; }

	public Task<IStickyNoteInventoryItem?> TryConsumeAsync(CancellationToken cancellationToken = default);

	IWallFurniture IFurnitureItem<IWallFurniture>.Furniture => this.Furniture;
}
