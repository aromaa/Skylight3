using Skylight.API.Game.Furniture.Floor.Wired.Triggers;
using Skylight.API.Game.Rooms.Items.Floor.Data.Wired.Trigger;
using Skylight.API.Game.Rooms.Items.Floor.Wired.Triggers;
using Skylight.API.Game.Rooms.Items.Interactions.Wired.Triggers;
using Skylight.Server.Game.Rooms.Items.Floor.Wired.Triggers;

namespace Skylight.Server.Game.Rooms.Items.Builders.Floor.Wired.Triggers;

internal sealed class UnitEnterRoomTriggerRoomItemBuilder : FloorItemBuilder<IUnitEnterRoomTriggerFurniture, IUnitEnterRoomTriggerRoomItem, UnitEnterRoomTriggerRoomItemBuilder, UnitEnterRoomTriggerRoomItemBuilder>,
	IUnitEnterRoomTriggerRoomItemDataBuilder<IUnitEnterRoomTriggerFurniture, IUnitEnterRoomTriggerRoomItem, UnitEnterRoomTriggerRoomItemBuilder, UnitEnterRoomTriggerRoomItemBuilder>
{
	private string? TriggerUsernameValue { get; set; }

	public IUnitEnterRoomTriggerRoomItemDataBuilder TriggerUsername(string triggerUsername)
	{
		this.TriggerUsernameValue = triggerUsername;

		return this;
	}

	public override IUnitEnterRoomTriggerRoomItem Build()
	{
		this.CheckValid();

		if (!this.RoomValue.ItemManager.TryGetInteractionHandler(out IUnitEnterRoomTriggerInteractionHandler? handler))
		{
			throw new Exception($"{typeof(IUnitEnterRoomTriggerInteractionHandler)} not found");
		}

		string? triggerUsername = this.TriggerUsernameValue;
		if (triggerUsername is null)
		{
			if (this.ExtraDataValue is not null)
			{
				triggerUsername = this.ExtraDataValue.RootElement.GetProperty("TriggerUsername").GetString();
				if (triggerUsername is null)
				{
					throw new InvalidOperationException("You must provide trigger username or extra data");
				}
			}
			else
			{
				throw new InvalidOperationException("You must provide trigger username or extra data");
			}
		}

		return new UnitEnterRoomTriggerRoomItem(this.RoomValue, this.IdValue, this.OwnerValue, this.FurnitureValue, this.PositionValue, this.DirectionValue, handler, triggerUsername);
	}
}
