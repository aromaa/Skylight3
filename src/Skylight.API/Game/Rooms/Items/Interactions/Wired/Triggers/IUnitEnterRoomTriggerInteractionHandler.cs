using Skylight.API.Game.Rooms.Items.Floor.Wired.Triggers;
using Skylight.API.Game.Rooms.Units;

namespace Skylight.API.Game.Rooms.Items.Interactions.Wired.Triggers;

public interface IUnitEnterRoomTriggerInteractionHandler : IRoomItemInteractionHandler
{
	public void OnPlace(IUnitEnterRoomTriggerRoomItem trigger);
	public void OnUpdate(IUnitEnterRoomTriggerRoomItem trigger);
	public void OnRemove(IUnitEnterRoomTriggerRoomItem trigger);

	public void OnEnterRoom(IUserRoomUnit unit);
}
