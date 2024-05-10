using Skylight.API.Game.Furniture;
using Skylight.API.Game.Furniture.Wall;

namespace Skylight.API.Game.Rooms.Items.Wall;

public interface IBasicWallRoomItem : IMultiStateWallRoomItem, IFurnitureItem<IBasicWallFurniture>
{
	public new IBasicWallFurniture Furniture { get; }

	IMultiStateWallFurniture IMultiStateWallRoomItem.Furniture => this.Furniture;
	IBasicWallFurniture IFurnitureItem<IBasicWallFurniture>.Furniture => this.Furniture;
}
