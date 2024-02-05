using Skylight.API.Game.Furniture;
using Skylight.API.Game.Furniture.Wall;
using Skylight.API.Numerics;

namespace Skylight.API.Game.Rooms.Items.Wall;

public interface IWallRoomItem : IRoomItem, IFurnitureItem<IWallFurniture>
{
	public new IWallFurniture Furniture { get; }

	public Point2D Location { get; }
	public Point2D Position { get; }

	IFurniture IFurnitureItem<IFurniture>.Furniture => this.Furniture;
	IWallFurniture IFurnitureItem<IWallFurniture>.Furniture => this.Furniture;
}
