using Skylight.API.Game.Furniture.Floor.Wired.Effects;
using Skylight.API.Game.Rooms.Items.Floor.Data.Wired.Effect;
using Skylight.API.Game.Rooms.Items.Floor.Wired.Effects;
using Skylight.API.Game.Rooms.Items.Interactions.Wired.Effects;
using Skylight.Server.Game.Rooms.Items.Floor.Wired.Effects;

namespace Skylight.Server.Game.Rooms.Items.Builders.Floor.Wired.Effects;

internal sealed class ShowMessageEffectRoomItemBuilder : WiredEffectRoomItemBuilder<IShowMessageEffectFurniture, IShowMessageEffectRoomItem, ShowMessageEffectRoomItemBuilder, ShowMessageEffectRoomItemBuilder>,
	IShowMessageEffectRoomItemDataBuilder<IShowMessageEffectFurniture, IShowMessageEffectRoomItem, ShowMessageEffectRoomItemBuilder, ShowMessageEffectRoomItemBuilder>
{
	private string? MessageValue { get; set; }

	public IShowMessageEffectRoomItemDataBuilder Message(string message)
	{
		this.MessageValue = message;

		return this;
	}

	protected override IShowMessageEffectRoomItem Build(int effectDelay)
	{
		this.CheckValid();

		if (!this.RoomValue.ItemManager.TryGetInteractionHandler(out IWiredEffectInteractionHandler? handler))
		{
			throw new Exception($"{typeof(IWiredEffectInteractionHandler)} not found");
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

		return new ShowMessageEffectRoomItem(this.RoomValue, this.IdValue, this.OwnerValue, this.FurnitureValue, this.PositionValue, this.DirectionValue, handler, message, effectDelay);
	}
}
