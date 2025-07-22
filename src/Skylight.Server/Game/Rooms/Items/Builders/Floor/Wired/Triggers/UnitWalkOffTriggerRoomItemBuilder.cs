using Skylight.API.Game.Furniture.Floor.Wired.Triggers;
using Skylight.API.Game.Rooms.Items.Floor.Wired.Triggers;
using Skylight.API.Game.Rooms.Items.Interactions.Wired.Triggers;
using Skylight.API.Registry;
using Skylight.Server.Game.Rooms.Items.Floor.Wired.Triggers;

namespace Skylight.Server.Game.Rooms.Items.Builders.Floor.Wired.Triggers;

internal sealed class UnitWalkOffTriggerRoomItemBuilder(IRegistryHolder registryHolder) : FloorItemBuilder<IUnitWalkOffTriggerFurniture, IUnitWalkOffTriggerRoomItem, UnitWalkOffTriggerRoomItemBuilder>
{
	private readonly IRegistryHolder registryHolder = registryHolder;

	public override IUnitWalkOffTriggerRoomItem Build()
	{
		this.CheckValid();

		if (!this.RoomValue.ItemManager.TryGetInteractionHandler(out IUnitWalkOffTriggerInteractionHandler? handler))
		{
			throw new Exception($"{typeof(IUnitWalkOffTriggerInteractionHandler)} not found");
		}

		return new UnitWalkOffTriggerRoomItem(this.registryHolder, this.RoomValue, this.IdValue, this.OwnerValue, this.FurnitureValue, this.PositionValue, this.DirectionValue, handler, null, this.ExtraDataValue);
	}
}
