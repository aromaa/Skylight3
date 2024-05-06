using Skylight.API.Game.Rooms.Items.Floor;
using Skylight.API.Game.Rooms.Items.Floor.Wired.Triggers;
using Skylight.API.Game.Rooms.Units;

namespace Skylight.API.Game.Rooms.Items.Interactions.Wired.Triggers;

public interface IUnitWalkOffTriggerInteractionHandler : IRoomItemInteractionHandler
{
	public void OnPlace(IUnitWalkOffTriggerRoomItem trigger);
	public void OnUpdate(IUnitWalkOffTriggerRoomItem trigger);
	public void OnRemove(IUnitWalkOffTriggerRoomItem trigger);

	public bool OnWalkOff(IUserRoomUnit user, IFloorRoomItem item);
}
