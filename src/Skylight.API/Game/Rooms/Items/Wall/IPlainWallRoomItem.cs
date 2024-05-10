using Skylight.API.Game.Furniture;
using Skylight.API.Game.Furniture.Wall;

namespace Skylight.API.Game.Rooms.Items.Wall;

public interface IPlainWallRoomItem : IWallRoomItem, IPlainRoomItem, IFurnitureItem<IPlainWallFurniture>
{
	public new IPlainWallFurniture Furniture { get; }

	IWallFurniture IWallRoomItem.Furniture => this.Furniture;
	IPlainFurniture IPlainRoomItem.Furniture => this.Furniture;
	IPlainWallFurniture IFurnitureItem<IPlainWallFurniture>.Furniture => this.Furniture;
}
