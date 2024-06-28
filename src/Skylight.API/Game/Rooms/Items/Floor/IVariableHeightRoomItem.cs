using Skylight.API.Game.Furniture;
using Skylight.API.Game.Furniture.Floor;

namespace Skylight.API.Game.Rooms.Items.Floor;

public interface IVariableHeightRoomItem : IMultiStateFloorItem, IInteractableRoomItem, IFurnitureItem<IVariableHeightFurniture>
{
	public new IVariableHeightFurniture Furniture { get; }

	IInteractableFurniture IInteractableRoomItem.Furniture => this.Furniture;
}
