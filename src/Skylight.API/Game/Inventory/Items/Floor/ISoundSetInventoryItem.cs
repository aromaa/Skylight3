using Skylight.API.Game.Furniture;
using Skylight.API.Game.Furniture.Floor;

namespace Skylight.API.Game.Inventory.Items.Floor;

public interface ISoundSetInventoryItem : IFloorInventoryItem, IFurnitureItem<ISoundSetFurniture>
{
	public new ISoundSetFurniture Furniture { get; }

	IFloorFurniture IFurnitureItem<IFloorFurniture>.Furniture => this.Furniture;
}
