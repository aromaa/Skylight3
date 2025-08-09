﻿using Microsoft.EntityFrameworkCore;
using Net.Communication.Attributes;
using Skylight.API.Game.Furniture.Floor;
using Skylight.API.Game.Inventory.Items.Floor;
using Skylight.API.Game.Rooms.Items.Interactions;
using Skylight.API.Game.Rooms.Private;
using Skylight.API.Game.Users;
using Skylight.Domain.Rooms.Sound;
using Skylight.Infrastructure;
using Skylight.Protocol.Packets.Data.Sound;
using Skylight.Protocol.Packets.Incoming.Sound;
using Skylight.Protocol.Packets.Manager;
using Skylight.Protocol.Packets.Outgoing.Sound;

namespace Skylight.Server.Game.Communication.Sound;

[PacketManagerRegister(typeof(IGamePacketManager))]
internal sealed partial class EditSongPacketHandler<T>(IDbContextFactory<SkylightContext> dbContextFactory) : UserPacketHandler<T>
	where T : IEditSongIncomingPacket
{
	private readonly IDbContextFactory<SkylightContext> dbContextFactory = dbContextFactory;

	internal override void Handle(IUser user, in T packet)
	{
		if (user.RoomSession?.Unit is not { Room: IPrivateRoom privateRoom } roomUnit)
		{
			return;
		}

		int songId = packet.SongId;

		user.Client.ScheduleTask(async _ =>
		{
			await using SkylightContext dbContext = await this.dbContextFactory.CreateDbContextAsync().ConfigureAwait(false);

			int songIdCaptured = songId;
			SongEntity? songEntity = await dbContext.Songs.FirstOrDefaultAsync(s => s.Id == songIdCaptured && s.UserId == user.Id).ConfigureAwait(false);
			if (songEntity is null)
			{
				return;
			}

			roomUnit.User.SendAsync(new SongInfoOutgoingPacket(songEntity.Id, songEntity.Name, songEntity.Data));

			privateRoom.PostTask(_ =>
			{
				if (!roomUnit.InRoom || !privateRoom.ItemManager.TryGetInteractionHandler(out ISoundMachineInteractionManager? handler) || handler.SoundMachine is not { } soundMachine)
				{
					return;
				}

				List<SoundSetData> filledSlots = [];
				foreach ((int slot, ISoundSetFurniture soundSet) in soundMachine.SoundSets)
				{
					filledSlots.Add(new SoundSetData(slot, soundSet.SoundSetId, soundSet.Samples));
				}

				user.SendAsync(new TraxSoundPackagesOutgoingPacket(soundMachine.Furniture.SoundSetSlotCount, filledSlots));

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
			});
		});
	}
}
