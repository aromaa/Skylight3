using System.Drawing;
using Skylight.API.Game.Furniture;
using Skylight.API.Game.Furniture.Wall;

namespace Skylight.API.Game.Rooms.Items.Wall.Builders;

public abstract class StickyNoteRoomItemBuilder
	: WallRoomItemBuilder, IFurnitureItemBuilder<IStickyNoteFurniture, IStickyNoteRoomItem>
{
	protected Color ColorValue { get; set; }
	protected string? TextValue { get; set; }

	public StickyNoteRoomItemBuilder Color(Color color)
	{
		this.ColorValue = color;

		return this;
	}

	public StickyNoteRoomItemBuilder Text(string text)
	{
		this.TextValue = text;

		return this;
	}

	public abstract override IStickyNoteRoomItem Build();
}
