using Net.Communication.Attributes;
using Skylight.API.Game.Furniture.Floor;
using Skylight.API.Game.Inventory.Items.Floor;
using Skylight.API.Game.Rooms.Items.Interactions;
using Skylight.API.Game.Users;
using Skylight.Protocol.Packets.Data.Sound;
using Skylight.Protocol.Packets.Incoming.Sound;
using Skylight.Protocol.Packets.Manager;
using Skylight.Protocol.Packets.Outgoing.Sound;

namespace Skylight.Server.Game.Communication.Sound;

[PacketManagerRegister(typeof(AbstractGamePacketManager))]
internal sealed partial class EjectSoundPackagePacketHandler<T> : UserPacketHandler<T>
	where T : IEjectSoundPackageIncomingPacket
{
	internal override void Handle(IUser user, in T packet)
	{
		if (user.RoomSession?.Unit is not { } unit)
		{
			return;
		}

		int slot = packet.Slot;

		unit.Room.PostTask(_ =>
		{
			if (!unit.InRoom || !unit.Room.IsOwner(user) || !unit.Room.ItemManager.TryGetInteractionHandler(out ISoundMachineInteractionManager? handler) || handler.SoundMachine is not { } soundMachine)
			{
				return;
			}

			soundMachine.RemoveSoundSet(slot);

			List<int> soundSets = [];
			foreach (IFloorInventoryItem item in unit.User.Inventory.FloorItems)
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

			unit.User.SendAsync(new UserSoundPackagesOutgoingPacket(soundSets));

			List<SoundSetData> filledSlots = [];
			foreach ((int slot, ISoundSetFurniture soundSet) in soundMachine.SoundSets)
			{
				filledSlots.Add(new SoundSetData(slot, soundSet.SoundSetId, soundSet.Samples));
			}

			unit.User.SendAsync(new TraxSoundPackagesOutgoingPacket(soundMachine.Furniture.SoundSetSlotCount, filledSlots));
		});
	}
}
