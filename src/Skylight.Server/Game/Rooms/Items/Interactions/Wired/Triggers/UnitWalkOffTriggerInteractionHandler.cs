using Skylight.API.Game.Furniture;
using Skylight.API.Game.Rooms.Items.Floor;
using Skylight.API.Game.Rooms.Items.Floor.Wired.Triggers;
using Skylight.API.Game.Rooms.Items.Interactions.Wired.Effects;
using Skylight.API.Game.Rooms.Items.Interactions.Wired.Triggers;
using Skylight.API.Game.Rooms.Units;
using Skylight.API.Numerics;

namespace Skylight.Server.Game.Rooms.Items.Interactions.Wired.Triggers;

internal sealed class UnitWalkOffTriggerInteractionHandler(IWiredEffectInteractionHandler wiredHandler) : IUnitWalkOffTriggerInteractionHandler
{
	private readonly IWiredEffectInteractionHandler wiredHandler = wiredHandler;

	public bool CanPlaceItem(IFurniture furniture, Point2D location) => true;

	private readonly HashSet<IUnitWalkOffTriggerRoomItem> triggers = [];

	public void OnPlace(IUnitWalkOffTriggerRoomItem trigger)
	{
		this.triggers.Add(trigger);
	}

	public void OnUpdate(IUnitWalkOffTriggerRoomItem trigger) => throw new NotImplementedException();

	public void OnRemove(IUnitWalkOffTriggerRoomItem trigger)
	{
		this.triggers.Remove(trigger);
	}

	public bool OnWalkOff(IUserRoomUnit user, IFloorRoomItem item)
	{
		bool result = false;
		foreach (IUnitWalkOffTriggerRoomItem trigger in this.triggers)
		{
			if (trigger.SelectedItems.Contains(item))
			{
				result = true;

				this.wiredHandler.TriggerStack(trigger, user);
			}
		}

		return result;
	}
}
