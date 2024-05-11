using Skylight.API.Game.Furniture.Floor;
using Skylight.API.Game.Rooms.Items.Floor;
using Skylight.API.Game.Rooms.Items.Interactions;
using Skylight.Server.Game.Rooms.Items.Floor;

namespace Skylight.Server.Game.Rooms.Items.Builders.Floor;

internal sealed class SoundMachineRoomItemBuilder : FloorItemBuilder<ISoundMachineFurniture, ISoundMachineRoomItem, SoundMachineRoomItemBuilder>
{
	public override ISoundMachineRoomItem Build()
	{
		this.CheckValid();

		if (!this.RoomValue.ItemManager.TryGetInteractionHandler(out ISoundMachineInteractionManager? handler))
		{
			throw new Exception($"{typeof(ISoundMachineInteractionManager)} not found");
		}

		return new SoundMachineRoomItem(this.RoomValue, this.IdValue, this.OwnerValue, this.FurnitureValue, this.PositionValue, this.DirectionValue, handler);
	}
}
