using Skylight.API.Game.Furniture;
using Skylight.API.Game.Furniture.Floor;

namespace Skylight.API.Game.Rooms.Items.Floor;

public interface IRollerRoomItem : IPlainFloorRoomItem, IFurnitureItem<IRollerFurniture>
{
	public new IRollerFurniture Furniture { get; }

	IPlainFloorFurniture IPlainFloorRoomItem.Furniture => this.Furniture;
	IRollerFurniture IFurnitureItem<IRollerFurniture>.Furniture => this.Furniture;
}
