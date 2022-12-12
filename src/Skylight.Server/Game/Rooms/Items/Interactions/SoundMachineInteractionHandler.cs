using Skylight.API.Game.Furniture;
using Skylight.API.Game.Furniture.Floor;
using Skylight.API.Game.Rooms.Items.Floor;
using Skylight.API.Game.Rooms.Items.Interactions;
using Skylight.API.Numerics;

namespace Skylight.Server.Game.Rooms.Items.Interactions;

internal sealed class SoundMachineInteractionHandler : ISoundMachineInteractionManager
{
	public ISoundMachineRoomItem? SoundMachine { get; private set; }

	public bool CanPlaceItem(IFurniture furniture, Point2D location)
	{
		return furniture is not ISoundMachineFurniture || this.SoundMachine is null;
	}

	public void OnPlace(ISoundMachineRoomItem soundMachine)
	{
		this.SoundMachine = soundMachine;
	}

	public void OnRemove(ISoundMachineRoomItem soundMachine)
	{
		if (this.SoundMachine == soundMachine)
		{
			this.SoundMachine = null;
		}
	}
}
