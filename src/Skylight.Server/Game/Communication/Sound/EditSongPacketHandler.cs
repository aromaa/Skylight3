using Microsoft.EntityFrameworkCore;
using Net.Communication.Attributes;
using Skylight.API.Game.Furniture.Floor;
using Skylight.API.Game.Inventory.Items.Floor;
using Skylight.API.Game.Rooms.Items.Interactions;
using Skylight.API.Game.Users;
using Skylight.Domain.Rooms.Sound;
using Skylight.Infrastructure;
using Skylight.Protocol.Packets.Data.Sound;
using Skylight.Protocol.Packets.Incoming.Sound;
using Skylight.Protocol.Packets.Manager;
using Skylight.Protocol.Packets.Outgoing.Sound;

namespace Skylight.Server.Game.Communication.Sound;

[PacketManagerRegister(typeof(AbstractGamePacketManager))]
internal sealed partial class EditSongPacketHandler<T> : UserPacketHandler<T>
	where T : IEditSongIncomingPacket
{
	private readonly IDbContextFactory<SkylightContext> dbContextFactory;

	public EditSongPacketHandler(IDbContextFactory<SkylightContext> dbContextFactory)
	{
		this.dbContextFactory = dbContextFactory;
	}

	internal override void Handle(IUser user, in T packet)
	{
		if (user.RoomSession?.Unit is not { } unit)
		{
			return;
		}

		int songId = packet.SongId;

		user.Client.ScheduleTask(async _ =>
		{
			await using SkylightContext dbContext = await this.dbContextFactory.CreateDbContextAsync().ConfigureAwait(false);

			int _songId = songId;
			SongEntity? songEntity = await dbContext.Songs.FirstOrDefaultAsync(s => s.Id == _songId).ConfigureAwait(false);
			if (songEntity is null)
			{
				return;
			}

			unit.User.SendAsync(new SongInfoOutgoingPacket(songEntity.Id, songEntity.Name, songEntity.Data));

			unit.Room.PostTask(_ =>
			{
				if (!unit.InRoom || !unit.Room.ItemManager.TryGetInteractionHandler(out ISoundMachineInteractionManager? handler) || handler.SoundMachine is not { } soundMachine)
				{
					return;
				}

				List<SoundSetData> filledSlots = new();
				foreach ((int slot, ISoundSetFurniture soundSet) in soundMachine.SoundSets)
				{
					filledSlots.Add(new SoundSetData(slot, soundSet.SoundSetId, soundSet.Samples));
				}

				unit.User.SendAsync(new TraxSoundPackagesOutgoingPacket(soundMachine.Furniture.SoundSetSlotCount, filledSlots));

				List<int> soundSets = new();
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
			});
		});
	}
}
