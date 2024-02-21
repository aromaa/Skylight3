using Skylight.API.Game.Furniture;
using Skylight.API.Game.Furniture.Floor;

namespace Skylight.API.Game.Rooms.Items.Floor;

public interface IFurniMaticGiftRoomItem : IFloorRoomItem, IFurnitureItem<IFurniMaticGiftFurniture>, IFurnitureItemData
{
	public new IFurniMaticGiftFurniture Furniture { get; }

	public DateTime RecycledAt { get; }

	IFloorFurniture IFloorRoomItem.Furniture => this.Furniture;
	IFurniMaticGiftFurniture IFurnitureItem<IFurniMaticGiftFurniture>.Furniture => this.Furniture;
}
