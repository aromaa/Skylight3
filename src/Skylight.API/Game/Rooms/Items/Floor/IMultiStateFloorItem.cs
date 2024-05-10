using Skylight.API.Game.Furniture;
using Skylight.API.Game.Furniture.Floor;

namespace Skylight.API.Game.Rooms.Items.Floor;

public interface IMultiStateFloorItem : IStatefulFloorRoomItem, IMultiStateRoomItem, IFurnitureItem<IMultiStateFloorFurniture>
{
	public new IMultiStateFloorFurniture Furniture { get; }

	IStatefulFurniture IStatefulRoomItem.Furniture => this.Furniture;
	IStatefulFloorFurniture IStatefulFloorRoomItem.Furniture => this.Furniture;
	IMultiStateFurniture IMultiStateRoomItem.Furniture => this.Furniture;
	IMultiStateFloorFurniture IFurnitureItem<IMultiStateFloorFurniture>.Furniture => this.Furniture;
}
