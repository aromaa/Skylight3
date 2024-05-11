using Skylight.API.Game.Furniture.Floor;
using Skylight.API.Game.Rooms.Items.Floor;
using Skylight.API.Game.Rooms.Items.Interactions;
using Skylight.Server.Game.Rooms.Items.Floor;

namespace Skylight.Server.Game.Rooms.Items.Builders.Floor;

internal sealed class StickyNotePoleRoomItemBuilder : FloorItemBuilder<IStickyNotePoleFurniture, IStickyNotePoleRoomItem, StickyNotePoleRoomItemBuilder>
{
	public override IStickyNotePoleRoomItem Build()
	{
		this.CheckValid();

		if (!this.RoomValue.ItemManager.TryGetInteractionHandler(out IStickyNoteInteractionHandler? handler))
		{
			throw new Exception($"{typeof(IStickyNoteInteractionHandler)} not found");
		}

		return new StickyNotePoleRoomItem(this.RoomValue, this.IdValue, this.OwnerValue, this.FurnitureValue, this.PositionValue, this.DirectionValue, handler);
	}
}
