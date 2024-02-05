using Skylight.API.Game.Furniture;
using Skylight.API.Game.Furniture.Floor;
using Skylight.API.Game.Rooms.Map;
using Skylight.API.Numerics;

namespace Skylight.API.Game.Rooms.Items.Floor;

public interface IFloorRoomItem : IRoomItem, IFurnitureItem<IFloorFurniture>
{
	public new IFloorFurniture Furniture { get; }

	public Point3D Position { get; }

	public int Direction { get; }
	public double Height { get; }

	public EffectiveTilesEnumerator EffectiveTiles { get; }

	public void OnMove(Point3D position, int direction);

	IFurniture IFurnitureItem<IFurniture>.Furniture => this.Furniture;
	IFloorFurniture IFurnitureItem<IFloorFurniture>.Furniture => this.Furniture;
}
