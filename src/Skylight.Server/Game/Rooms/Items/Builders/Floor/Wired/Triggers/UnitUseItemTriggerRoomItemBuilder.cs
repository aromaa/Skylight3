using Skylight.API.Game.Furniture.Floor.Wired.Triggers;
using Skylight.API.Game.Rooms.Items.Floor.Wired.Triggers;
using Skylight.API.Game.Rooms.Items.Interactions.Wired.Triggers;
using Skylight.API.Registry;
using Skylight.Server.Game.Rooms.Items.Floor.Wired.Triggers;

namespace Skylight.Server.Game.Rooms.Items.Builders.Floor.Wired.Triggers;

internal sealed class UnitUseItemTriggerRoomItemBuilder(IRegistryHolder registryHolder) : FloorItemBuilder<IUnitUseItemTriggerFurniture, IUnitUseItemTriggerRoomItem, UnitUseItemTriggerRoomItemBuilder>
{
	private readonly IRegistryHolder registryHolder = registryHolder;

	public override IUnitUseItemTriggerRoomItem Build()
	{
		this.CheckValid();

		if (!this.RoomValue.ItemManager.TryGetInteractionHandler(out IUnitUseItemTriggerInteractionHandler? handler))
		{
			throw new Exception($"{typeof(IUnitUseItemTriggerInteractionHandler)} not found");
		}

		return new UnitUseItemTriggerRoomItem(this.registryHolder, this.RoomValue, this.IdValue, this.OwnerValue, this.FurnitureValue, this.PositionValue, this.DirectionValue, handler, null, this.ExtraDataValue);
	}
}
