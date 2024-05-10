using System.Drawing;
using Skylight.API.Game.Furniture;
using Skylight.API.Game.Furniture.Wall;

namespace Skylight.API.Game.Rooms.Items.Wall;

public interface IStickyNoteRoomItem : IComplexWallRoomItem, IFurnitureItemData, IFurnitureItem<IStickyNoteFurniture>
{
	public new IStickyNoteFurniture Furniture { get; }

	public Color Color { get; set; }
	public string Text { get; set; }

	IComplexWallFurniture IComplexWallRoomItem.Furniture => this.Furniture;
	IStickyNoteFurniture IFurnitureItem<IStickyNoteFurniture>.Furniture => this.Furniture;
}
