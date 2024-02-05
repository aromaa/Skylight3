using Skylight.API.Game.Furniture;
using Skylight.API.Game.Furniture.Floor;

namespace Skylight.API.Game.Rooms.Items.Floor;

public interface IMultiStateFloorItem : IFloorRoomItem, IMultiStateRoomItem, IFurnitureItem<IMultiStateFloorFurniture>
{
	public new IMultiStateFloorFurniture Furniture { get; }

	IFloorFurniture IFloorRoomItem.Furniture => this.Furniture;
	IMultiStateFurniture IMultiStateRoomItem.Furniture => this.Furniture;
	IMultiStateFloorFurniture IFurnitureItem<IMultiStateFloorFurniture>.Furniture => this.Furniture;
	IFurniture IFurnitureItem<IFurniture>.Furniture => this.Furniture; //Diamond
}
