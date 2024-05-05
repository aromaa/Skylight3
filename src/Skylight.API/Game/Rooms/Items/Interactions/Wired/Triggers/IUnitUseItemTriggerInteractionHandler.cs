using Skylight.API.Game.Rooms.Items.Floor.Wired.Triggers;
using Skylight.API.Game.Rooms.Units;

namespace Skylight.API.Game.Rooms.Items.Interactions.Wired.Triggers;

public interface IUnitUseItemTriggerInteractionHandler : IRoomItemInteractionHandler
{
	public void OnPlace(IUnitUseItemTriggerRoomItem trigger);
	public void OnUpdate(IUnitUseItemTriggerRoomItem trigger);
	public void OnRemove(IUnitUseItemTriggerRoomItem trigger);

	public void OnUse(IUserRoomUnit unit, IRoomItem item);
}
