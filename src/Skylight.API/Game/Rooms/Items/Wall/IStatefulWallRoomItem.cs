using Skylight.API.Game.Furniture;
using Skylight.API.Game.Furniture.Wall;

namespace Skylight.API.Game.Rooms.Items.Wall;

public interface IStatefulWallRoomItem : IWallRoomItem, IStatefulRoomItem, IFurnitureItem<IStatefulWallFurniture>
{
	public new IStatefulWallFurniture Furniture { get; }

	IWallFurniture IWallRoomItem.Furniture => this.Furniture;
	IStatefulFurniture IStatefulRoomItem.Furniture => this.Furniture;
	IStatefulWallFurniture IFurnitureItem<IStatefulWallFurniture>.Furniture => this.Furniture;
}
