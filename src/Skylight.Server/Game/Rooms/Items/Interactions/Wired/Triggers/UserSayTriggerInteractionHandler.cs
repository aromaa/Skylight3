using Skylight.API.Game.Furniture;
using Skylight.API.Game.Rooms.Items.Floor.Wired.Triggers;
using Skylight.API.Game.Rooms.Items.Interactions.Wired.Effects;
using Skylight.API.Game.Rooms.Items.Interactions.Wired.Triggers;
using Skylight.API.Game.Rooms.Units;
using Skylight.API.Numerics;

namespace Skylight.Server.Game.Rooms.Items.Interactions.Wired.Triggers;

internal sealed class UserSayTriggerInteractionHandler(IWiredEffectInteractionHandler wiredHandler) : IUserSayTriggerInteractionHandler
{
	private readonly IWiredEffectInteractionHandler wiredHandler = wiredHandler;

	private readonly HashSet<IUserSayTriggerRoomItem> triggers = [];

	public bool CanPlaceItem(IFurniture furniture, Point2D location) => true;

	public void OnPlace(IUserSayTriggerRoomItem trigger)
	{
		this.triggers.Add(trigger);
	}

	public void OnUpdate(IUserSayTriggerRoomItem trigger)
	{
		//Todo
	}

	public void OnRemove(IUserSayTriggerRoomItem trigger)
	{
		this.triggers.Remove(trigger);
	}

	public bool OnSay(IUserRoomUnit user, string message)
	{
		bool result = false;
		foreach (IUserSayTriggerRoomItem trigger in this.triggers)
		{
			if (message.Contains(trigger.Message))
			{
				result = true;

				this.wiredHandler.TriggerStack(trigger, user);
			}
		}

		return result;
	}
}
