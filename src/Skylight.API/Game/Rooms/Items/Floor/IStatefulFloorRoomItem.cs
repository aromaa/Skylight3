using Skylight.API.Game.Furniture;
using Skylight.API.Game.Furniture.Floor;

namespace Skylight.API.Game.Rooms.Items.Floor;

public interface IStatefulFloorRoomItem : IFloorRoomItem, IStatefulRoomItem, IFurnitureItem<IStatefulFloorFurniture>
{
	public new IStatefulFloorFurniture Furniture { get; }

	IStatefulFurniture IStatefulRoomItem.Furniture => this.Furniture;
	IStatefulFloorFurniture IFurnitureItem<IStatefulFloorFurniture>.Furniture => this.Furniture;
}
