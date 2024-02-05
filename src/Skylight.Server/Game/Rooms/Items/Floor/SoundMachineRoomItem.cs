using System.Collections.Immutable;
using Skylight.API.Game.Furniture.Floor;
using Skylight.API.Game.Rooms;
using Skylight.API.Game.Rooms.Items.Floor;
using Skylight.API.Game.Rooms.Items.Interactions;
using Skylight.API.Game.Users;
using Skylight.API.Numerics;

namespace Skylight.Server.Game.Rooms.Items.Floor;

internal sealed class SoundMachineRoomItem : FloorRoomItem, ISoundMachineRoomItem
{
	public override ISoundMachineFurniture Furniture { get; }

	private readonly ISoundMachineInteractionManager handler;

	private readonly ISoundSetFurniture?[] soundSetSlots;

	public SoundMachineRoomItem(IRoom room, int id, IUserInfo owner, ISoundMachineFurniture furniture, Point3D position, int direction, ISoundMachineInteractionManager handler)
		: base(room, id, owner, position, direction)
	{
		this.Furniture = furniture;

		this.handler = handler;

		this.soundSetSlots = new ISoundSetFurniture[4];
	}

	public override double Height => this.Furniture.DefaultHeight;

	public ImmutableArray<(int Slot, ISoundSetFurniture SoundSet)> SoundSets => this.soundSetSlots.Select((s, i) => (i + 1, s)).Where(s => s.s is not null).ToImmutableArray()!;

	public bool HasSoundSet(int soundSetId) => this.soundSetSlots.Any(s => s?.SoundSetId == soundSetId);

	public void AddSoundSet(int slot, ISoundSetFurniture soundSet) => this.soundSetSlots[slot - 1] = soundSet;
	public void RemoveSoundSet(int slot) => this.soundSetSlots[slot - 1] = null;

	public override void OnPlace() => this.handler.OnPlace(this);
	public override void OnRemove() => this.handler.OnRemove(this);
}
