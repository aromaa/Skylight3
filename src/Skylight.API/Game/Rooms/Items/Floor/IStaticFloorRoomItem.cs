using Skylight.API.Game.Furniture;
using Skylight.API.Game.Furniture.Floor;

namespace Skylight.API.Game.Rooms.Items.Floor;

public interface IStaticFloorRoomItem : IPlainFloorRoomItem, IStaticRoomItem, IFurnitureItem<IStaticFloorFurniture>
{
	public new IStaticFloorFurniture Furniture { get; }

	IPlainFloorFurniture IPlainFloorRoomItem.Furniture => this.Furniture;
	IStaticFurniture IStaticRoomItem.Furniture => this.Furniture;
	IStaticFloorFurniture IFurnitureItem<IStaticFloorFurniture>.Furniture => this.Furniture;
}
