using Skylight.API.Game.Furniture;

namespace Skylight.API.Game.Rooms.Items;

public interface IBasicRoomItem : IMultiStateRoomItem, IFurnitureItem<IBasicFurniture>
{
	public new IBasicFurniture Furniture { get; }

	IMultiStateFurniture IMultiStateRoomItem.Furniture => this.Furniture;
	IBasicFurniture IFurnitureItem<IBasicFurniture>.Furniture => this.Furniture;
}
