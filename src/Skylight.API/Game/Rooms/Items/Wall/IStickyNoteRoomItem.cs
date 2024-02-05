using System.Drawing;
using Skylight.API.Game.Furniture;
using Skylight.API.Game.Furniture.Wall;

namespace Skylight.API.Game.Rooms.Items.Wall;

public interface IStickyNoteRoomItem : IWallRoomItem, IFurnitureItem<IStickyNoteFurniture>, IFurnitureData<(Color Color, string Text)>
{
	public new IStickyNoteFurniture Furniture { get; }

	public Color Color { get; set; }
	public string Text { get; set; }

	IWallFurniture IWallRoomItem.Furniture => this.Furniture;
	IStickyNoteFurniture IFurnitureItem<IStickyNoteFurniture>.Furniture => this.Furniture;
}
