using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Skylight.API.Game.Furniture.Floor;
using Skylight.API.Game.Furniture.Floor.Wired.Effects;
using Skylight.API.Game.Rooms.Items.Floor;
using Skylight.API.Game.Rooms.Items.Floor.Builders;
using Skylight.API.Game.Rooms.Items.Interactions.Wired.Effects;

namespace Skylight.Server.Game.Rooms.Items.Floor.Wired.Effects.Builders;

internal sealed class ShowMessageEffectRoomItemBuilderImpl : FloorRoomItemBuilder
{
	private IShowMessageEffectFurniture? FurnitureValue { get; set; }

	private string? MessageValue { get; set; }
	private int EffectDelayValue { get; set; }

	public override FloorRoomItemBuilder Furniture(IFloorFurniture furniture)
	{
		this.FurnitureValue = (IShowMessageEffectFurniture)furniture;

		return this;
	}

	public override FloorRoomItemBuilder ExtraData(JsonDocument extraData)
	{
		if (extraData.RootElement.TryGetProperty("Message", out JsonElement messageValue))
		{
			this.MessageValue = messageValue.GetString();
		}

		if (extraData.RootElement.TryGetProperty("EffectDelay", out JsonElement effectDelayValue))
		{
			this.EffectDelayValue = effectDelayValue.GetInt32();
		}

		return this;
	}

	public override IFloorRoomItem Build()
	{
		this.CheckValid();

		if (!this.RoomValue.ItemManager.TryGetInteractionHandler(out IWiredEffectInteractionHandler? handler))
		{
			throw new Exception($"{typeof(IWiredEffectInteractionHandler)} not found");
		}

		return new ShowMessageEffectRoomItem(this.RoomValue, this.ItemIdValue, this.OwnerValue, this.FurnitureValue, this.PositionValue, this.DirectionValue, handler)
		{
			Message = this.MessageValue ?? string.Empty,
			EffectDelay = this.EffectDelayValue
		};
	}

	[MemberNotNull(nameof(this.FurnitureValue))]
	protected override void CheckValid()
	{
		base.CheckValid();

		ArgumentNullException.ThrowIfNull(this.FurnitureValue);
	}
}
