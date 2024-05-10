using Skylight.API.Game.Furniture;
using Skylight.API.Game.Furniture.Wall;

namespace Skylight.API.Game.Rooms.Items.Wall;

public interface IComplexWallRoomItem : IWallRoomItem, IComplexRoomItem, IFurnitureItem<IComplexWallFurniture>
{
	public new IComplexWallFurniture Furniture { get; }

	IWallFurniture IWallRoomItem.Furniture => this.Furniture;
	IComplexFurniture IComplexRoomItem.Furniture => this.Furniture;
	IComplexWallFurniture IFurnitureItem<IComplexWallFurniture>.Furniture => this.Furniture;
}
