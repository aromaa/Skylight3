using Skylight.API.Game.Furniture;
using Skylight.API.Game.Furniture.Wall;

namespace Skylight.API.Game.Rooms.Items.Wall;

public interface IMultiStateWallRoomItem : IStatefulWallRoomItem, IMultiStateRoomItem, IFurnitureItem<IMultiStateWallFurniture>
{
	public new IMultiStateWallFurniture Furniture { get; }

	IStatefulFurniture IStatefulRoomItem.Furniture => this.Furniture;
	IStatefulWallFurniture IStatefulWallRoomItem.Furniture => this.Furniture;
	IMultiStateFurniture IMultiStateRoomItem.Furniture => this.Furniture;
	IMultiStateWallFurniture IFurnitureItem<IMultiStateWallFurniture>.Furniture => this.Furniture;
}
