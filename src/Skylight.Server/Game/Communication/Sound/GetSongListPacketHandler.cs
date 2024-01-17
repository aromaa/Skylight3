using Microsoft.EntityFrameworkCore;
using Net.Communication.Attributes;
using Skylight.API.Game.Rooms.Items.Interactions;
using Skylight.API.Game.Users;
using Skylight.Infrastructure;
using Skylight.Protocol.Packets.Data.Sound;
using Skylight.Protocol.Packets.Incoming.Sound;
using Skylight.Protocol.Packets.Manager;
using Skylight.Protocol.Packets.Outgoing.Sound;

namespace Skylight.Server.Game.Communication.Sound;

[PacketManagerRegister(typeof(AbstractGamePacketManager))]
internal sealed partial class GetSongListPacketHandler<T> : UserPacketHandler<T>
	where T : IGetSongListIncomingPacket
{
	private readonly IDbContextFactory<SkylightContext> dbContextFactory;

	public GetSongListPacketHandler(IDbContextFactory<SkylightContext> dbContextFactory)
	{
		this.dbContextFactory = dbContextFactory;
	}

	internal override void Handle(IUser user, in T packet)
	{
		if (user.RoomSession?.Unit is not { } unit)
		{
			return;
		}

		user.Client.ScheduleTask(async client =>
		{
			int soundMachineId = await unit.Room.ScheduleTask(room =>
			{
				if (!unit.InRoom || !room.ItemManager.TryGetInteractionHandler(out ISoundMachineInteractionManager? handler) || handler.SoundMachine is not { } soundMachine)
				{
					return 0;
				}

				return soundMachine.Id;
			}).ConfigureAwait(false);

			if (soundMachineId == 0)
			{
				return;
			}

			await using SkylightContext dbContext = await this.dbContextFactory.CreateDbContextAsync().ConfigureAwait(false);

			List<SongData> songs = await dbContext.Songs
				.Where(s => s.ItemId == soundMachineId)
				.Select(s => new SongData(s.Id, s.Name, s.Length, false))
				.ToListAsync()
				.ConfigureAwait(false);

			client.SendAsync(new SongListOutgoingPacket(songs));
		});
	}
}
