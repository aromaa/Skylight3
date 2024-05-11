using Skylight.API.Game.Furniture.Floor.Wired.Triggers;
using Skylight.API.Game.Rooms.Items.Floor.Wired.Triggers;
using Skylight.API.Game.Rooms.Items.Interactions.Wired.Triggers;
using Skylight.Server.Game.Rooms.Items.Floor.Wired.Triggers;

namespace Skylight.Server.Game.Rooms.Items.Builders.Floor.Wired.Triggers;

internal sealed class UnitUseItemTriggerRoomItemBuilder : FloorItemBuilder<IUnitUseItemTriggerFurniture, IUnitUseItemTriggerRoomItem, UnitUseItemTriggerRoomItemBuilder>
{
	public override IUnitUseItemTriggerRoomItem Build()
	{
		this.CheckValid();

		if (!this.RoomValue.ItemManager.TryGetInteractionHandler(out IUnitUseItemTriggerInteractionHandler? handler))
		{
			throw new Exception($"{typeof(IUnitUseItemTriggerInteractionHandler)} not found");
		}

		return new UnitUseItemTriggerRoomItem(this.RoomValue, this.IdValue, this.OwnerValue, this.FurnitureValue, this.PositionValue, this.DirectionValue, handler, null, this.ExtraDataValue);
	}
}
