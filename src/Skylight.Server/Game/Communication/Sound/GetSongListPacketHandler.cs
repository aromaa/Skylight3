using Microsoft.EntityFrameworkCore;
using Net.Communication.Attributes;
using Skylight.API.Game.Rooms.Items;
using Skylight.API.Game.Rooms.Items.Interactions;
using Skylight.API.Game.Rooms.Private;
using Skylight.API.Game.Users;
using Skylight.API.Registry;
using Skylight.Infrastructure;
using Skylight.Protocol.Packets.Data.Sound;
using Skylight.Protocol.Packets.Incoming.Sound;
using Skylight.Protocol.Packets.Manager;
using Skylight.Protocol.Packets.Outgoing.Sound;

namespace Skylight.Server.Game.Communication.Sound;

[PacketManagerRegister(typeof(IGamePacketManager))]
internal sealed partial class GetSongListPacketHandler<T>(IRegistryHolder registryHolder, IDbContextFactory<SkylightContext> dbContextFactory) : UserPacketHandler<T>
	where T : IGetSongListIncomingPacket
{
	// TODO: Support other domains
	private readonly IRoomItemDomain normalRoomItemDomain = RoomItemDomains.Normal.Get(registryHolder);

	private readonly IDbContextFactory<SkylightContext> dbContextFactory = dbContextFactory;

	internal override void Handle(IUser user, in T packet)
	{
		if (user.RoomSession?.Unit is not { Room: IPrivateRoom privateRoom } roomUnit)
		{
			return;
		}

		user.Client.ScheduleTask(async client =>
		{
			RoomItemId soundMachineId = await privateRoom.ScheduleTask(_ =>
			{
				if (!roomUnit.InRoom || !privateRoom.ItemManager.TryGetInteractionHandler(out ISoundMachineInteractionManager? handler) || handler.SoundMachine is not { } soundMachine)
				{
					return default;
				}

				return soundMachine.Id;
			}).ConfigureAwait(false);

			if (soundMachineId == default || soundMachineId.Domain != this.normalRoomItemDomain)
			{
				return;
			}

			await using SkylightContext dbContext = await this.dbContextFactory.CreateDbContextAsync().ConfigureAwait(false);

			List<SongData> songs = await dbContext.Songs
				.Where(s => s.ItemId == soundMachineId.Id)
				.Select(s => new SongData(s.Id, s.Name, s.Length, false))
				.ToListAsync()
				.ConfigureAwait(false);

			client.SendAsync(new SongListOutgoingPacket(songs));
		});
	}
}
