using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Skylight.API.Game.Furniture.Floor;
using Skylight.API.Game.Furniture.Floor.Wired.Triggers;
using Skylight.API.Game.Rooms.Items.Floor;
using Skylight.API.Game.Rooms.Items.Floor.Builders;
using Skylight.API.Game.Rooms.Items.Interactions.Wired.Triggers;

namespace Skylight.Server.Game.Rooms.Items.Floor.Wired.Triggers.Builders;

internal sealed class UnitEnterRoomTriggerRoomItemBuilderImpl : FloorRoomItemBuilder
{
	private IUnitEnterRoomTriggerFurniture? FurnitureValue { get; set; }

	private string? TriggerUsernameValue { get; set; }

	public override FloorRoomItemBuilder Furniture(IFloorFurniture furniture)
	{
		this.FurnitureValue = (IUnitEnterRoomTriggerFurniture)furniture;

		return this;
	}

	public override FloorRoomItemBuilder ExtraData(JsonDocument extraData)
	{
		if (extraData.RootElement.TryGetProperty("TriggerUsername", out JsonElement triggerUsernameValue))
		{
			this.TriggerUsernameValue = triggerUsernameValue.GetString();
		}

		return this;
	}

	public override IFloorRoomItem Build()
	{
		this.CheckValid();

		if (!this.RoomValue.ItemManager.TryGetInteractionHandler(out IUnitEnterRoomTriggerInteractionHandler? handler))
		{
			throw new Exception($"{typeof(IUnitEnterRoomTriggerInteractionHandler)} not found");
		}

		return new UnitEnterRoomTriggerRoomItem(this.RoomValue, this.ItemIdValue, this.OwnerValue, this.FurnitureValue, this.PositionValue, this.DirectionValue, handler)
		{
			TriggerUsername = this.TriggerUsernameValue
		};
	}

	[MemberNotNull(nameof(this.FurnitureValue))]
	protected override void CheckValid()
	{
		base.CheckValid();

		ArgumentNullException.ThrowIfNull(this.FurnitureValue);
	}
}
