using Skylight.API.Game.Rooms.Items.Floor;

namespace Skylight.API.Game.Rooms.Items.Interactions;

public interface IRollerInteractionHandler : IRoomItemInteractionHandler
{
	public void OnPlace(IRollerRoomItem roller);
	public void OnRemove(IRollerRoomItem roller);
}
