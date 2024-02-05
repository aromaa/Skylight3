using Skylight.API.Game.Furniture;

namespace Skylight.API.Game.Rooms.Items;

public interface IMultiStateRoomItem : IInteractableRoomItem, IFurnitureItem<IMultiStateFurniture>
{
	public new IMultiStateFurniture Furniture { get; }

	public int State { get; }

	IInteractableFurniture IInteractableRoomItem.Furniture => this.Furniture;
	IMultiStateFurniture IFurnitureItem<IMultiStateFurniture>.Furniture => this.Furniture;
}
