using System.Diagnostics.CodeAnalysis;
using Skylight.API.Game.Furniture.Floor;
using Skylight.API.Game.Rooms.Items.Floor;
using Skylight.API.Game.Rooms.Items.Floor.Builders;
using Skylight.API.Game.Rooms.Items.Interactions;

namespace Skylight.Server.Game.Rooms.Items.Floor.Builders;

internal sealed class StickyNotePoleRoomItemBuilderImpl
	: FloorRoomItemBuilder
{
	private IStickyNotePoleFurniture? FurnitureValue { get; set; }

	public override StickyNotePoleRoomItemBuilderImpl Furniture(IFloorFurniture furniture)
	{
		this.FurnitureValue = (IStickyNotePoleFurniture)furniture;

		return this;
	}

	public override IFloorRoomItem Build()
	{
		this.CheckValid();

		if (!this.RoomValue.ItemManager.TryGetInteractionHandler(out IStickyNoteInteractionHandler? handler))
		{
			throw new Exception($"{typeof(IStickyNoteInteractionHandler)} not found");
		}

		return new StickyNotePoleRoomItem(this.RoomValue, this.ItemIdValue, this.OwnerValue, this.FurnitureValue, this.PositionValue, this.DirectionValue, handler);
	}

	[MemberNotNull(nameof(this.FurnitureValue))]
	protected override void CheckValid()
	{
		base.CheckValid();

		ArgumentNullException.ThrowIfNull(this.FurnitureValue);
	}
}
