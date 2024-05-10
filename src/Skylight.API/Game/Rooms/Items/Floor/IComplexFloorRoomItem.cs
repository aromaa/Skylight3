using Skylight.API.Game.Furniture;
using Skylight.API.Game.Furniture.Floor;

namespace Skylight.API.Game.Rooms.Items.Floor;

public interface IComplexFloorRoomItem : IFloorRoomItem, IComplexRoomItem, IFurnitureItem<IComplexFloorFurniture>
{
	public new IComplexFloorFurniture Furniture { get; }

	IFloorFurniture IFloorRoomItem.Furniture => this.Furniture;
	IComplexFurniture IComplexRoomItem.Furniture => this.Furniture;
	IComplexFloorFurniture IFurnitureItem<IComplexFloorFurniture>.Furniture => this.Furniture;
}
