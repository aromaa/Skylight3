using Skylight.API.Game.Furniture;

namespace Skylight.API.Game.Rooms.Items;

public interface IMultiStateRoomItem : IStatefulRoomItem, IFurnitureItem<IMultiStateFurniture>
{
	public new IMultiStateFurniture Furniture { get; }

	public new int State { get; set; }

	int IStatefulRoomItem.State => this.State;
	IStatefulFurniture IStatefulRoomItem.Furniture => this.Furniture;
	IMultiStateFurniture IFurnitureItem<IMultiStateFurniture>.Furniture => this.Furniture;
}
