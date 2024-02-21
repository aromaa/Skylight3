using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Skylight.API.Game.Furniture.Wall;
using Skylight.API.Game.Rooms.Items.Interactions;
using Skylight.API.Game.Rooms.Items.Wall;
using Skylight.API.Game.Rooms.Items.Wall.Builders;

namespace Skylight.Server.Game.Rooms.Items.Wall.Builders;

internal sealed class StickyNoteRoomItemBuilderImpl
	: StickyNoteRoomItemBuilder
{
	private IStickyNoteFurniture? FurnitureValue { get; set; }

	public override StickyNoteRoomItemBuilderImpl Furniture(IWallFurniture furniture)
	{
		this.FurnitureValue = (IStickyNoteFurniture)furniture;

		return this;
	}

	public override StickyNoteRoomItemBuilderImpl ExtraData(JsonDocument extraData)
	{
		this.ColorValue = System.Drawing.Color.FromArgb(extraData.RootElement.GetProperty("Color").GetInt32());
		this.TextValue = extraData.RootElement.GetProperty("Text").GetString();

		return this;
	}

	public override IStickyNoteRoomItem Build()
	{
		this.CheckValid();

		if (!this.RoomValue.ItemManager.TryGetInteractionHandler(out IStickyNoteInteractionHandler? handler))
		{
			throw new Exception($"{typeof(IStickyNoteInteractionHandler)} not found");
		}

		return new PostItRoomItem(this.RoomValue, this.ItemIdValue, this.OwnerValue, this.FurnitureValue, this.LocationValue, this.PositionValue, 0, this.ColorValue == default ? this.FurnitureValue.DefaultColor : this.ColorValue, this.TextValue ?? string.Empty, handler);
	}

	[MemberNotNull(nameof(this.FurnitureValue))]
	protected override void CheckValid()
	{
		base.CheckValid();

		ArgumentNullException.ThrowIfNull(this.FurnitureValue);
	}
}
