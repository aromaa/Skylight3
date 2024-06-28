using Skylight.API.Game.Furniture;
using Skylight.API.Game.Rooms.Units;

namespace Skylight.API.Game.Rooms.Items;

public interface IInteractableRoomItem : IRoomItem, IFurnitureItem<IInteractableFurniture>
{
	public new IInteractableFurniture Furniture { get; }

	public bool Interact(IUserRoomUnit unit, int state);

	IInteractableFurniture IFurnitureItem<IInteractableFurniture>.Furniture => this.Furniture;
}
