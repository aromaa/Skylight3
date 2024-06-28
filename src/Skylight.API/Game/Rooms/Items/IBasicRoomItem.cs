using Skylight.API.Game.Furniture;

namespace Skylight.API.Game.Rooms.Items;

public interface IBasicRoomItem : IMultiStateRoomItem, IInteractableRoomItem, IFurnitureItem<IBasicFurniture>
{
	public new IBasicFurniture Furniture { get; }

	IMultiStateFurniture IMultiStateRoomItem.Furniture => this.Furniture;
	IInteractableFurniture IInteractableRoomItem.Furniture => this.Furniture;
	IBasicFurniture IFurnitureItem<IBasicFurniture>.Furniture => this.Furniture;
}
