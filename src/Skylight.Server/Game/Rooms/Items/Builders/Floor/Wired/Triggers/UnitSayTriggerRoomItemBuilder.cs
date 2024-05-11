using Skylight.API.Game.Furniture.Floor.Wired.Triggers;
using Skylight.API.Game.Rooms.Items.Floor.Data.Wired.Trigger;
using Skylight.API.Game.Rooms.Items.Floor.Wired.Triggers;
using Skylight.API.Game.Rooms.Items.Interactions.Wired.Triggers;
using Skylight.Server.Game.Rooms.Items.Floor.Wired.Triggers;

namespace Skylight.Server.Game.Rooms.Items.Builders.Floor.Wired.Triggers;

internal sealed class UnitSayTriggerRoomItemBuilder : FloorItemBuilder<IUnitSayTriggerFurniture, IUnitSayTriggerRoomItem, UnitSayTriggerRoomItemBuilder, UnitSayTriggerRoomItemBuilder>,
	IUnitSayTriggerRoomItemDataBuilder<IUnitSayTriggerFurniture, IUnitSayTriggerRoomItem, UnitSayTriggerRoomItemBuilder, UnitSayTriggerRoomItemBuilder>
{
	private string? MessageValue { get; set; }

	public IUnitSayTriggerRoomItemDataBuilder Message(string message)
	{
		this.MessageValue = message;

		return this;
	}

	public override IUnitSayTriggerRoomItem Build()
	{
		this.CheckValid();

		if (!this.RoomValue.ItemManager.TryGetInteractionHandler(out IUnitSayTriggerInteractionHandler? handler))
		{
			throw new Exception($"{typeof(IUnitSayTriggerInteractionHandler)} not found");
		}

		string? message = this.MessageValue;
		if (message is null)
		{
			if (this.ExtraDataValue is not null)
			{
				message = this.ExtraDataValue.RootElement.GetProperty("Message").GetString();
				if (message is null)
				{
					throw new InvalidOperationException("You must provide message or extra data");
				}
			}
			else
			{
				throw new InvalidOperationException("You must provide message or extra data");
			}
		}

		return new UnitSayTriggerRoomItem(this.RoomValue, this.IdValue, this.OwnerValue, this.FurnitureValue, this.PositionValue, this.DirectionValue, handler, message);
	}
}
