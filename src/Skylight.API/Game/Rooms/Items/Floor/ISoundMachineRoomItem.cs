using System.Collections.Immutable;
using Skylight.API.Game.Furniture;
using Skylight.API.Game.Furniture.Floor;

namespace Skylight.API.Game.Rooms.Items.Floor;

public interface ISoundMachineRoomItem : IFloorRoomItem, IFurnitureItem<ISoundMachineFurniture>
{
	public new ISoundMachineFurniture Furniture { get; }

	public ImmutableArray<(int Slot, ISoundSetFurniture SoundSet)> SoundSets { get; }

	public bool HasSoundSet(int soundSetId);

	public void AddSoundSet(int slot, ISoundSetFurniture soundSet);
	public void RemoveSoundSet(int slot);

	IFloorFurniture IFloorRoomItem.Furniture => this.Furniture;
	ISoundMachineFurniture IFurnitureItem<ISoundMachineFurniture>.Furniture => this.Furniture;
}
