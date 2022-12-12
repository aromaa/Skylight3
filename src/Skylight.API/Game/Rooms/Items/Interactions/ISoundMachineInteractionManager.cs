using Skylight.API.Game.Rooms.Items.Floor;

namespace Skylight.API.Game.Rooms.Items.Interactions;

public interface ISoundMachineInteractionManager : IRoomItemInteractionHandler
{
	public ISoundMachineRoomItem? SoundMachine { get; }

	public void OnPlace(ISoundMachineRoomItem soundMachine);

	public void OnRemove(ISoundMachineRoomItem soundMachine);
}
