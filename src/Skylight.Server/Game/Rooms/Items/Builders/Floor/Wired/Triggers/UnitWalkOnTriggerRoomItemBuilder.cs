using Skylight.API.Game.Furniture.Floor.Wired.Triggers;
using Skylight.API.Game.Rooms.Items.Floor.Wired.Triggers;
using Skylight.API.Game.Rooms.Items.Interactions.Wired.Triggers;
using Skylight.API.Registry;
using Skylight.Server.Game.Rooms.Items.Floor.Wired.Triggers;

namespace Skylight.Server.Game.Rooms.Items.Builders.Floor.Wired.Triggers;

internal sealed class UnitWalkOnTriggerRoomItemBuilder(IRegistryHolder registryHolder) : FloorItemBuilder<IUnitWalkOnTriggerFurniture, IUnitWalkOnTriggerRoomItem, UnitWalkOnTriggerRoomItemBuilder>
{
	private readonly IRegistryHolder registryHolder = registryHolder;

	public override IUnitWalkOnTriggerRoomItem Build()
	{
		this.CheckValid();

		if (!this.RoomValue.ItemManager.TryGetInteractionHandler(out IUnitWalkOnTriggerInteractionHandler? handler))
		{
			throw new Exception($"{typeof(IUnitWalkOnTriggerInteractionHandler)} not found");
		}

		return new UnitWalkOnTriggerRoomItem(this.RoomValue, this.registryHolder, this.IdValue, this.OwnerValue, this.FurnitureValue, this.PositionValue, this.DirectionValue, handler, null, this.ExtraDataValue);
	}
}
