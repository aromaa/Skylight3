using Skylight.API.Game.Rooms.Items.Floor;
using Skylight.API.Game.Rooms.Items.Floor.Wired.Triggers;
using Skylight.API.Game.Rooms.Units;

namespace Skylight.API.Game.Rooms.Items.Interactions.Wired.Triggers;

public interface IUnitWalkOnTriggerInteractionHandler : IRoomItemInteractionHandler
{
	public void OnPlace(IUnitWalkOnTriggerRoomItem trigger);
	public void OnUpdate(IUnitWalkOnTriggerRoomItem trigger);
	public void OnRemove(IUnitWalkOnTriggerRoomItem trigger);

	public bool OnWalkOn(IUserRoomUnit user, IFloorRoomItem item);
}
