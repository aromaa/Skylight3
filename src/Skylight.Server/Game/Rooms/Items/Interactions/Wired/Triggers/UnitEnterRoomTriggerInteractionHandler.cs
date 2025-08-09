using Skylight.API.Game.Furniture;
using Skylight.API.Game.Rooms.Items.Floor.Wired.Triggers;
using Skylight.API.Game.Rooms.Items.Interactions.Wired.Effects;
using Skylight.API.Game.Rooms.Items.Interactions.Wired.Triggers;
using Skylight.API.Game.Rooms.Units;
using Skylight.API.Numerics;

namespace Skylight.Server.Game.Rooms.Items.Interactions.Wired.Triggers;

internal sealed class UnitEnterRoomTriggerInteractionHandler(IWiredEffectInteractionHandler wiredHandler) : IUnitEnterRoomTriggerInteractionHandler
{
	private readonly IWiredEffectInteractionHandler wiredHandler = wiredHandler;

	private readonly HashSet<IUnitEnterRoomTriggerRoomItem> triggers = [];

	public bool CanPlaceItem(IFurniture furniture, Point2D location) => true;

	public void OnPlace(IUnitEnterRoomTriggerRoomItem trigger)
	{
		this.triggers.Add(trigger);
	}

	public void OnUpdate(IUnitEnterRoomTriggerRoomItem trigger)
	{
		//Todo
	}

	public void OnRemove(IUnitEnterRoomTriggerRoomItem trigger)
	{
		this.triggers.Remove(trigger);
	}

	public void OnEnterRoom(IUserRoomUnit unit)
	{
		foreach (IUnitEnterRoomTriggerRoomItem trigger in this.triggers)
		{
			if (trigger.TriggerUsername is null || trigger.TriggerUsername == unit.User.Info.Username)
			{
				this.wiredHandler.TriggerStack(trigger, unit);
			}
		}
	}
}
