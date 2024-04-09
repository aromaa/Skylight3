using Skylight.API.Game.Rooms.Items.Floor.Wired.Triggers;
using Skylight.API.Game.Rooms.Units;

namespace Skylight.API.Game.Rooms.Items.Interactions.Wired.Triggers;

public interface IUserSayTriggerInteractionHandler : IRoomItemInteractionHandler
{
	public void OnPlace(IUserSayTriggerRoomItem trigger);
	public void OnUpdate(IUserSayTriggerRoomItem trigger);
	public void OnRemove(IUserSayTriggerRoomItem trigger);

	public bool OnSay(IUserRoomUnit user, string message);
}
