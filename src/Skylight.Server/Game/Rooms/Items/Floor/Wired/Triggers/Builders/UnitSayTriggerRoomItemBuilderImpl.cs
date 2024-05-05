using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Skylight.API.Game.Furniture.Floor;
using Skylight.API.Game.Furniture.Floor.Wired.Triggers;
using Skylight.API.Game.Rooms.Items.Floor;
using Skylight.API.Game.Rooms.Items.Floor.Builders;
using Skylight.API.Game.Rooms.Items.Interactions.Wired.Triggers;

namespace Skylight.Server.Game.Rooms.Items.Floor.Wired.Triggers.Builders;

internal sealed class UnitSayTriggerRoomItemBuilderImpl
	: FloorRoomItemBuilder
{
	private IUnitSayTriggerFurniture? FurnitureValue { get; set; }

	private string? MessageValue { get; set; }

	public override FloorRoomItemBuilder Furniture(IFloorFurniture furniture)
	{
		this.FurnitureValue = (IUnitSayTriggerFurniture)furniture;

		return this;
	}

	public override FloorRoomItemBuilder ExtraData(JsonDocument extraData)
	{
		if (extraData.RootElement.TryGetProperty("Message", out JsonElement messageValue))
		{
			this.MessageValue = messageValue.GetString();
		}

		return this;
	}

	public override IFloorRoomItem Build()
	{
		this.CheckValid();

		if (!this.RoomValue.ItemManager.TryGetInteractionHandler(out IUnitSayTriggerInteractionHandler? handler))
		{
			throw new Exception($"{typeof(IUnitSayTriggerInteractionHandler)} not found");
		}

		return new UnitSayTriggerRoomItem(this.RoomValue, this.ItemIdValue, this.OwnerValue, this.FurnitureValue, this.PositionValue, this.DirectionValue, handler)
		{
			Message = this.MessageValue ?? string.Empty
		};
	}

	[MemberNotNull(nameof(this.FurnitureValue))]
	protected override void CheckValid()
	{
		base.CheckValid();

		ArgumentNullException.ThrowIfNull(this.FurnitureValue);
	}
}
