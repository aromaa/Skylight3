using Skylight.API.Game.Furniture;
using Skylight.API.Game.Furniture.Floor;

namespace Skylight.API.Game.Inventory.Items.Floor;

public interface IFloorInventoryItem : IFurnitureInventoryItem, IFurnitureItem<IFloorFurniture>
{
	public new IFloorFurniture Furniture { get; }

	IFurniture IFurnitureItem<IFurniture>.Furniture => this.Furniture;
}
