using Skylight.API.Game.Furniture;
using Skylight.API.Game.Furniture.Floor;
using Skylight.API.Game.Furniture.Floor.Wired;

namespace Skylight.API.Game.Rooms.Items.Floor.Wired;

public interface IWiredRoomItem : IComplexFloorRoomItem, IInteractableRoomItem, IFurnitureItem<IWiredFurniture>
{
	public new IWiredFurniture Furniture { get; }

	IComplexFloorFurniture IComplexFloorRoomItem.Furniture => this.Furniture;
	IInteractableFurniture IInteractableRoomItem.Furniture => this.Furniture;
	IWiredFurniture IFurnitureItem<IWiredFurniture>.Furniture => this.Furniture;
}
