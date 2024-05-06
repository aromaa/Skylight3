using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Skylight.API.Game.Furniture.Floor;
using Skylight.API.Game.Furniture.Floor.Wired.Triggers;
using Skylight.API.Game.Rooms.Items.Floor;
using Skylight.API.Game.Rooms.Items.Floor.Builders;
using Skylight.API.Game.Rooms.Items.Interactions.Wired.Triggers;

namespace Skylight.Server.Game.Rooms.Items.Floor.Wired.Triggers.Builders;

internal sealed class UnitWalkOffTriggerRoomItemBuilderImpl : FloorRoomItemBuilder
{
	private IUnitWalkOffTriggerFurniture? FurnitureValue { get; set; }

	private JsonDocument? ExtraDataValue { get; set; }

	public override FloorRoomItemBuilder Furniture(IFloorFurniture furniture)
	{
		this.FurnitureValue = (IUnitWalkOffTriggerFurniture)furniture;

		return this;
	}

	public override FloorRoomItemBuilder ExtraData(JsonDocument extraData)
	{
		this.ExtraDataValue = extraData;

		return this;
	}

	public override IFloorRoomItem Build()
	{
		this.CheckValid();

		if (!this.RoomValue.ItemManager.TryGetInteractionHandler(out IUnitWalkOffTriggerInteractionHandler? handler))
		{
			throw new Exception($"{typeof(IUnitWalkOffTriggerInteractionHandler)} not found");
		}

		return new UnitWalkOffTriggerRoomItem(this.RoomValue, this.ItemIdValue, this.OwnerValue, this.FurnitureValue, this.PositionValue, this.DirectionValue, handler, null, this.ExtraDataValue);
	}

	[MemberNotNull(nameof(this.FurnitureValue))]
	protected override void CheckValid()
	{
		base.CheckValid();

		ArgumentNullException.ThrowIfNull(this.FurnitureValue);
	}
}
