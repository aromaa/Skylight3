using Skylight.API.Game.Furniture;
using Skylight.API.Game.Furniture.Wall;

namespace Skylight.API.Game.Inventory.Items.Wall;

public interface IWallInventoryItem : IFurnitureInventoryItem, IFurnitureItem<IWallFurniture>
{
	public new IWallFurniture Furniture { get; }

	IFurniture IFurnitureItem<IFurniture>.Furniture => this.Furniture;
}
