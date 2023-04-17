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
internal sealed class InsertSoundPackagePacketHandler<T> : UserPacketHandler<T>
	where T : IInsertSoundPackageIncomingPacket
{
	internal override void Handle(IUser user, in T packet)
	{
		if (user.RoomSession?.Unit is not { } unit)
		{
			return;
		}

		unit.Room.ScheduleTask(static (room, state) =>
		{
			if (!state.RoomUnit.InRoom || !state.RoomUnit.Room.ItemManager.TryGetInteractionHandler(out ISoundMachineInteractionManager? handler) || handler.SoundMachine is not { } soundMachine)
			{
				return;
			}

			foreach (IFloorInventoryItem item in state.RoomUnit.User.Inventory.FloorItems)
			{
				if (item is not ISoundSetInventoryItem soundSet)
				{
					continue;
				}

				if (state.SoundSetId == soundSet.Furniture.SoundSetId)
				{
					soundMachine.AddSoundSet(state.Slot, soundSet.Furniture);

					break;
				}
			}

			List<SoundSetData> filledSlots = new();
			foreach ((int slot, ISoundSetFurniture soundSet) in soundMachine.SoundSets)
			{
				filledSlots.Add(new SoundSetData(slot, soundSet.SoundSetId, soundSet.Samples));
			}

			state.RoomUnit.User.SendAsync(new TraxSoundPackagesOutgoingPacket(soundMachine.Furniture.SoundSetSlotCount, filledSlots));
		}, (RoomUnit: unit, Slot: packet.Slot, SoundSetId: packet.SoundSetId));
	}
}
