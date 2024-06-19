using Skylight.API.Game.Furniture;
using Skylight.API.Game.Furniture.Floor;

namespace Skylight.API.Game.Rooms.Items.Floor;

public interface IBasicFloorRoomItem : IMultiStateFloorItem, IBasicRoomItem, IFurnitureItem<IBasicFloorFurniture>
{
	public new IBasicFloorFurniture Furniture { get; }

	IMultiStateFurniture IMultiStateRoomItem.Furniture => this.Furniture;
	IBasicFurniture IBasicRoomItem.Furniture => this.Furniture;
	IBasicFloorFurniture IFurnitureItem<IBasicFloorFurniture>.Furniture => this.Furniture;
}
