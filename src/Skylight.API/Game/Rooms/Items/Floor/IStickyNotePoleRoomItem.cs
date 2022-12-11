using Skylight.API.Game.Furniture;
using Skylight.API.Game.Furniture.Floor;

namespace Skylight.API.Game.Rooms.Items.Floor;

public interface IStickyNotePoleRoomItem : IFloorRoomItem, IFurnitureItem<IStickyNotePoleFurniture>
{
	public new IStickyNotePoleFurniture Furniture { get; }

	IFloorFurniture IFurnitureItem<IFloorFurniture>.Furniture => this.Furniture;
}
