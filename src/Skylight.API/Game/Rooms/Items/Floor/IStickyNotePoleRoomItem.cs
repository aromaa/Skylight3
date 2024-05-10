using Skylight.API.Game.Furniture;
using Skylight.API.Game.Furniture.Floor;

namespace Skylight.API.Game.Rooms.Items.Floor;

public interface IStickyNotePoleRoomItem : IPlainFloorRoomItem, IFurnitureItem<IStickyNotePoleFurniture>
{
	public new IStickyNotePoleFurniture Furniture { get; }

	IPlainFloorFurniture IPlainFloorRoomItem.Furniture => this.Furniture;
	IStickyNotePoleFurniture IFurnitureItem<IStickyNotePoleFurniture>.Furniture => this.Furniture;
}
