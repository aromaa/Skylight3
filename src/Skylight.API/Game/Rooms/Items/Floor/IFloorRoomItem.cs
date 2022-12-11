using Skylight.API.Game.Furniture;
using Skylight.API.Game.Furniture.Floor;
using Skylight.API.Numerics;

namespace Skylight.API.Game.Rooms.Items.Floor;

public interface IFloorRoomItem : IRoomItem, IFurnitureItem<IFloorFurniture>
{
	public new IFloorFurniture Furniture { get; }

	public Point3D Position { get; }
	public int Direction { get; }

	public void OnMove(Point3D position, int direction);

	IFurniture IFurnitureItem<IFurniture>.Furniture => this.Furniture;
}
