using Skylight.API.Game.Furniture;
using Skylight.API.Game.Furniture.Floor;

namespace Skylight.API.Game.Inventory.Items.Floor;

public interface IFurniMaticGiftInventoryItem : IFloorInventoryItem, IFurnitureItem<IFurniMaticGiftFurniture>, IFurnitureItemData
{
	public new IFurniMaticGiftFurniture Furniture { get; }

	public DateTime RecycledAt { get; }

	IFloorFurniture IFurnitureItem<IFloorFurniture>.Furniture => this.Furniture;
}
