using System.Drawing;
using Skylight.API.Game.Furniture;
using Skylight.API.Game.Furniture.Wall;
using Skylight.API.Game.Rooms.Items;
using Skylight.API.Game.Rooms.Items.Interactions;
using Skylight.API.Game.Rooms.Items.Wall;
using Skylight.API.Game.Rooms.Items.Wall.Data;
using Skylight.Server.Game.Rooms.Items.Wall;

namespace Skylight.Server.Game.Rooms.Items.Builders.Wall;

internal sealed class StickyNoteRoomItemBuilder : WallRoomItemBuilder<IStickyNoteFurniture, IStickyNoteRoomItem, StickyNoteRoomItemBuilder>,
	IStickyNoteRoomItemDataBuilder<IStickyNoteFurniture, IStickyNoteRoomItem, StickyNoteRoomItemBuilder, StickyNoteRoomItemBuilder>,
	IFurnitureItemDataBuilder<IStickyNoteFurniture, RoomItemId, IStickyNoteRoomItem, StickyNoteRoomItemBuilder, StickyNoteRoomItemBuilder>
{
	private Color ColorValue { get; set; }
	private string? TextValue { get; set; }

	public IStickyNoteRoomItemDataBuilder Color(Color color)
	{
		this.ColorValue = color;

		return this;
	}

	public IStickyNoteRoomItemDataBuilder Text(string text)
	{
		this.TextValue = text;

		return this;
	}

	public override IStickyNoteRoomItem Build()
	{
		this.CheckValid();

		if (!this.RoomValue.ItemManager.TryGetInteractionHandler(out IStickyNoteInteractionHandler? handler))
		{
			throw new Exception($"{typeof(IStickyNoteInteractionHandler)} not found");
		}

		Color color = this.ColorValue;
		if (color == default)
		{
			color = this.ExtraDataValue is null
				? this.FurnitureValue.DefaultColor
				: System.Drawing.Color.FromArgb(this.ExtraDataValue.RootElement.GetProperty("Color").GetInt32());
		}

		string? text = this.TextValue;
		if (text is null)
		{
			if (this.ExtraDataValue is not null)
			{
				text = this.ExtraDataValue.RootElement.GetProperty("Text").GetString();
				if (text is null)
				{
					throw new NotSupportedException("Extra data contained null 'Text' section");
				}
			}
			else
			{
				text = string.Empty;
			}
		}

		return new PostItRoomItem(this.RoomValue, this.IdValue, this.OwnerValue, this.FurnitureValue, this.LocationValue, this.PositionValue, 0, color, text, handler);
	}

	public StickyNoteRoomItemBuilder CompleteData() => this;
	public StickyNoteRoomItemBuilder Data() => this;
}
