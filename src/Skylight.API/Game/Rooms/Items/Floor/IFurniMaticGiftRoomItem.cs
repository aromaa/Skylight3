using Skylight.API.Game.Furniture;
using Skylight.API.Game.Furniture.Floor;

namespace Skylight.API.Game.Rooms.Items.Floor;

public interface IFurniMaticGiftRoomItem : IFloorRoomItem, IFurnitureItem<IFurniMaticGiftFurniture>, IFurnitureData<DateTimeOffset>
{
	public new IFurniMaticGiftFurniture Furniture { get; }

	public DateTimeOffset RecycledAt { get; }

	IFloorFurniture IFurnitureItem<IFloorFurniture>.Furniture => this.Furniture;
}
