using Net.Communication.Attributes;
using Skylight.API.Game.Furniture.Floor;
using Skylight.API.Game.Inventory.Items.Floor;
using Skylight.API.Game.Rooms.Items.Interactions;
using Skylight.API.Game.Rooms.Private;
using Skylight.API.Game.Users;
using Skylight.Protocol.Packets.Data.Sound;
using Skylight.Protocol.Packets.Incoming.Sound;
using Skylight.Protocol.Packets.Manager;
using Skylight.Protocol.Packets.Outgoing.Sound;

namespace Skylight.Server.Game.Communication.Sound;

[PacketManagerRegister(typeof(IGamePacketManager))]
internal sealed partial class EjectSoundPackagePacketHandler<T> : UserPacketHandler<T>
	where T : IEjectSoundPackageIncomingPacket
{
	internal override void Handle(IUser user, in T packet)
	{
		if (user.RoomSession?.Unit is not { Room: IPrivateRoom privateRoom } roomUnit || !privateRoom.IsOwner(user))
		{
			return;
		}

		int slot = packet.Slot;

		privateRoom.PostTask(_ =>
		{
			if (!roomUnit.InRoom || !privateRoom.IsOwner(user) || !privateRoom.ItemManager.TryGetInteractionHandler(out ISoundMachineInteractionManager? handler) || handler.SoundMachine is not { } soundMachine)
			{
				return;
			}

			soundMachine.RemoveSoundSet(slot);

			List<int> soundSets = [];
			foreach (IFloorInventoryItem item in user.Inventory.FloorItems)
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

			user.SendAsync(new UserSoundPackagesOutgoingPacket(soundSets));

			List<SoundSetData> filledSlots = [];
			foreach ((int slot, ISoundSetFurniture soundSet) in soundMachine.SoundSets)
			{
				filledSlots.Add(new SoundSetData(slot, soundSet.SoundSetId, soundSet.Samples));
			}

			user.SendAsync(new TraxSoundPackagesOutgoingPacket(soundMachine.Furniture.SoundSetSlotCount, filledSlots));
		});
	}
}
