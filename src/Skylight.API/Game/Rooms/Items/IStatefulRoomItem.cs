using Skylight.API.Game.Furniture;

namespace Skylight.API.Game.Rooms.Items;

public interface IStatefulRoomItem : IRoomItem, IFurnitureItem<IStatefulFurniture>
{
	public new IStatefulFurniture Furniture { get; }

	public int State { get; }

	IStatefulFurniture IFurnitureItem<IStatefulFurniture>.Furniture => this.Furniture;
}
