using Skylight.API.Game.Furniture;
using Skylight.API.Game.Furniture.Floor;

namespace Skylight.API.Game.Rooms.Items.Floor;

public interface IPlainFloorRoomItem : IFloorRoomItem, IPlainRoomItem, IFurnitureItem<IPlainFloorFurniture>
{
	public new IPlainFloorFurniture Furniture { get; }

	IFloorFurniture IFloorRoomItem.Furniture => this.Furniture;
	IPlainFurniture IPlainRoomItem.Furniture => this.Furniture;
	IPlainFloorFurniture IFurnitureItem<IPlainFloorFurniture>.Furniture => this.Furniture;
}
