using Skylight.API.Game.Furniture;
using Skylight.API.Game.Rooms.Items;
using Skylight.API.Game.Rooms.Items.Floor.Wired.Triggers;
using Skylight.API.Game.Rooms.Items.Interactions.Wired.Effects;
using Skylight.API.Game.Rooms.Items.Interactions.Wired.Triggers;
using Skylight.API.Game.Rooms.Units;
using Skylight.API.Numerics;

namespace Skylight.Server.Game.Rooms.Items.Interactions.Wired.Triggers;

internal sealed class UnitUseItemTriggerInteractionHandler(IWiredEffectInteractionHandler wiredHandler) : IUnitUseItemTriggerInteractionHandler
{
	private readonly IWiredEffectInteractionHandler wiredHandler = wiredHandler;

	private readonly HashSet<IUnitUseItemTriggerRoomItem> triggers = [];

	public bool CanPlaceItem(IFurniture furniture, Point2D location) => true;

	public void OnPlace(IUnitUseItemTriggerRoomItem trigger)
	{
		this.triggers.Add(trigger);
	}

	public void OnUpdate(IUnitUseItemTriggerRoomItem trigger)
	{
		//Todo
	}

	public void OnRemove(IUnitUseItemTriggerRoomItem trigger)
	{
		this.triggers.Remove(trigger);
	}

	public void OnUse(IUserRoomUnit unit, IRoomItem item)
	{
		foreach (IUnitUseItemTriggerRoomItem trigger in this.triggers)
		{
			if (trigger.SelectedItems.Contains(item))
			{
				this.wiredHandler.TriggerStack(trigger, unit);
			}
		}
	}
}
