using Skylight.API.Game.Furniture.Floor.Wired.Triggers;
using Skylight.API.Game.Rooms.Items.Floor.Wired.Triggers;
using Skylight.API.Game.Rooms.Items.Interactions.Wired.Triggers;
using Skylight.Server.Game.Rooms.Items.Floor.Wired.Triggers;

namespace Skylight.Server.Game.Rooms.Items.Builders.Floor.Wired.Triggers;

internal sealed class UnitWalkOffTriggerRoomItemBuilder : FloorItemBuilder<IUnitWalkOffTriggerFurniture, IUnitWalkOffTriggerRoomItem, UnitWalkOffTriggerRoomItemBuilder>
{
	public override IUnitWalkOffTriggerRoomItem Build()
	{
		this.CheckValid();

		if (!this.RoomValue.ItemManager.TryGetInteractionHandler(out IUnitWalkOffTriggerInteractionHandler? handler))
		{
			throw new Exception($"{typeof(IUnitWalkOffTriggerInteractionHandler)} not found");
		}

		return new UnitWalkOffTriggerRoomItem(this.RoomValue, this.IdValue, this.OwnerValue, this.FurnitureValue, this.PositionValue, this.DirectionValue, handler, null, this.ExtraDataValue);
	}
}
