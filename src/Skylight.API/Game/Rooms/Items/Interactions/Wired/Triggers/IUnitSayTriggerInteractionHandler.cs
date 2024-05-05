using Skylight.API.Game.Rooms.Items.Floor.Wired.Triggers;
using Skylight.API.Game.Rooms.Units;

namespace Skylight.API.Game.Rooms.Items.Interactions.Wired.Triggers;

public interface IUnitSayTriggerInteractionHandler : IRoomItemInteractionHandler
{
	public void OnPlace(IUnitSayTriggerRoomItem trigger);
	public void OnUpdate(IUnitSayTriggerRoomItem trigger);
	public void OnRemove(IUnitSayTriggerRoomItem trigger);

	public bool OnSay(IUserRoomUnit user, string message);
}
