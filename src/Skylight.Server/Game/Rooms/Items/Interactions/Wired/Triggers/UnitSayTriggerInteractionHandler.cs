using Skylight.API.Game.Furniture;
using Skylight.API.Game.Rooms.Items.Floor.Wired.Triggers;
using Skylight.API.Game.Rooms.Items.Interactions.Wired.Effects;
using Skylight.API.Game.Rooms.Items.Interactions.Wired.Triggers;
using Skylight.API.Game.Rooms.Units;
using Skylight.API.Numerics;

namespace Skylight.Server.Game.Rooms.Items.Interactions.Wired.Triggers;

internal sealed class UnitSayTriggerInteractionHandler(IWiredEffectInteractionHandler wiredHandler) : IUnitSayTriggerInteractionHandler
{
	private readonly IWiredEffectInteractionHandler wiredHandler = wiredHandler;

	private readonly HashSet<IUnitSayTriggerRoomItem> triggers = [];

	public bool CanPlaceItem(IFurniture furniture, Point2D location) => true;

	public void OnPlace(IUnitSayTriggerRoomItem trigger)
	{
		this.triggers.Add(trigger);
	}

	public void OnUpdate(IUnitSayTriggerRoomItem trigger)
	{
		//Todo
	}

	public void OnRemove(IUnitSayTriggerRoomItem trigger)
	{
		this.triggers.Remove(trigger);
	}

	public bool OnSay(IUserRoomUnit user, string message)
	{
		bool result = false;
		foreach (IUnitSayTriggerRoomItem trigger in this.triggers)
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
