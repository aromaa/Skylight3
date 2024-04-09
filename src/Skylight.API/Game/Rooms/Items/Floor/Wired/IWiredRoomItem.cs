using Skylight.API.Game.Furniture;
using Skylight.API.Game.Furniture.Floor;
using Skylight.API.Game.Furniture.Floor.Wired;

namespace Skylight.API.Game.Rooms.Items.Floor.Wired;

public interface IWiredRoomItem : IFloorRoomItem, IInteractableRoomItem, IFurnitureItem<IWiredFurniture>
{
	public new IWiredFurniture Furniture { get; }

	IFloorFurniture IFloorRoomItem.Furniture => this.Furniture;
	IInteractableFurniture IInteractableRoomItem.Furniture => this.Furniture;
	IFurniture IFurnitureItem<IFurniture>.Furniture => this.Furniture;
	IInteractableFurniture IFurnitureItem<IInteractableFurniture>.Furniture => this.Furniture;
	IWiredFurniture IFurnitureItem<IWiredFurniture>.Furniture => this.Furniture;
}
