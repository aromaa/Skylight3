using Skylight.API.Game.Furniture;
using Skylight.API.Game.Furniture.Wall;

namespace Skylight.API.Game.Rooms.Items.Wall;

public interface IBasicWallRoomItem : IWallRoomItem, IFurnitureItem<IBasicWallFurniture>
{
	public new IBasicWallFurniture Furniture { get; }

	IWallFurniture IWallRoomItem.Furniture => this.Furniture;
	IBasicWallFurniture IFurnitureItem<IBasicWallFurniture>.Furniture => this.Furniture;
}
