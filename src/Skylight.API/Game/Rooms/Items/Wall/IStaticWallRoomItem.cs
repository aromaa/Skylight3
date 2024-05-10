using Skylight.API.Game.Furniture;
using Skylight.API.Game.Furniture.Wall;

namespace Skylight.API.Game.Rooms.Items.Wall;

public interface IStaticWallRoomItem : IPlainWallRoomItem, IStaticRoomItem, IFurnitureItem<IStaticWallFurniture>
{
	public new IStaticWallFurniture Furniture { get; }

	IPlainWallFurniture IPlainWallRoomItem.Furniture => this.Furniture;
	IStaticFurniture IStaticRoomItem.Furniture => this.Furniture;
	IStaticWallFurniture IFurnitureItem<IStaticWallFurniture>.Furniture => this.Furniture;
}
