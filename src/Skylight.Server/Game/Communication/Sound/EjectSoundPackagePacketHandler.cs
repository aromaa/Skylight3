using System.Runtime.InteropServices;
using Net.Communication.Attributes;
using Skylight.API.Game.Furniture.Floor;
using Skylight.API.Game.Inventory.Items.Floor;
using Skylight.API.Game.Rooms;
using Skylight.API.Game.Rooms.Items.Interactions;
using Skylight.API.Game.Rooms.Units;
using Skylight.API.Game.Users;
using Skylight.Protocol.Packets.Data.Sound;
using Skylight.Protocol.Packets.Incoming.Sound;
using Skylight.Protocol.Packets.Manager;
using Skylight.Protocol.Packets.Outgoing.Sound;

namespace Skylight.Server.Game.Communication.Sound;

[PacketManagerRegister(typeof(AbstractGamePacketManager))]
internal sealed class EjectSoundPackagePacketHandler<T> : UserPacketHandler<T>
	where T : IEjectSoundPackageIncomingPacket
{
	internal override void Handle(IUser user, in T packet)
	{
		if (user.RoomSession?.Unit is not { } unit)
		{
			return;
		}

		unit.Room.ScheduleTask(new EjectSoundPackageTask
		{
			Unit = unit,

			Slot = packet.Slot
		});
	}

	[StructLayout(LayoutKind.Auto)]
	private readonly struct EjectSoundPackageTask : IRoomTask
	{
		internal IUserRoomUnit Unit { get; init; }

		internal int Slot { get; init; }

		public void Execute(IRoom room)
		{
			if (!this.Unit.InRoom || !this.Unit.Room.ItemManager.TryGetInteractionHandler(out ISoundMachineInteractionManager? handler) || handler.SoundMachine is not { } soundMachine)
			{
				return;
			}

			soundMachine.RemoveSoundSet(this.Slot);

			List<int> soundSets = new();
			foreach (IFloorInventoryItem item in this.Unit.User.Inventory.FloorItems)
			{
				if (item is not ISoundSetInventoryItem soundSet)
				{
					continue;
				}

				if (soundMachine.HasSoundSet(soundSet.Furniture.SoundSetId))
				{
					continue;
				}

				soundSets.Add(soundSet.Furniture.SoundSetId);
			}

			this.Unit.User.SendAsync(new UserSoundPackagesOutgoingPacket(soundSets));

			List<SoundSetData> filledSlots = new();
			foreach ((int slot, ISoundSetFurniture soundSet) in soundMachine.SoundSets)
			{
				filledSlots.Add(new SoundSetData(slot, soundSet.SoundSetId, soundSet.Samples));
			}

			this.Unit.User.SendAsync(new TraxSoundPackagesOutgoingPacket(soundMachine.Furniture.SoundSetSlotCount, filledSlots));
		}
	}
}
