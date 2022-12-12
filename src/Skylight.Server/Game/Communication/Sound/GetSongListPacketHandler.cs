using System.Runtime.InteropServices;
using Microsoft.EntityFrameworkCore;
using Net.Communication.Attributes;
using Skylight.API.Game.Clients;
using Skylight.API.Game.Rooms;
using Skylight.API.Game.Rooms.Items.Interactions;
using Skylight.API.Game.Rooms.Units;
using Skylight.API.Game.Users;
using Skylight.Infrastructure;
using Skylight.Protocol.Packets.Data.Sound;
using Skylight.Protocol.Packets.Incoming.Sound;
using Skylight.Protocol.Packets.Manager;
using Skylight.Protocol.Packets.Outgoing.Sound;

namespace Skylight.Server.Game.Communication.Sound;

[PacketManagerRegister(typeof(AbstractGamePacketManager))]
internal sealed class GetSongListPacketHandler<T> : UserPacketHandler<T>
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

		user.Client.ScheduleTask(new GetSongListTask
		{
			DbContextFactory = this.dbContextFactory,

			Unit = unit
		});
	}

	[StructLayout(LayoutKind.Auto)]
	private readonly struct GetSongListTask : IClientTask, IRoomTask<int>
	{
		internal IDbContextFactory<SkylightContext> DbContextFactory { get; init; }

		internal IUserRoomUnit Unit { get; init; }

		public async Task ExecuteAsync(IClient client)
		{
			int soundMachineId = await this.Unit.Room.ScheduleTaskAsync<GetSongListTask, int>(this).ConfigureAwait(false);
			if (soundMachineId == 0)
			{
				return;
			}

			await using SkylightContext dbContext = await this.DbContextFactory.CreateDbContextAsync().ConfigureAwait(false);

			List<SongData> songs = await dbContext.Songs
				.Where(s => s.ItemId == soundMachineId)
				.Select(s => new SongData(s.Id, s.Name, s.Length, false))
				.ToListAsync()
				.ConfigureAwait(false);

			client.SendAsync(new SongListOutgoingPacket(songs));
		}

		public int Execute(IRoom room)
		{
			if (!this.Unit.InRoom || !this.Unit.Room.ItemManager.TryGetInteractionHandler(out ISoundMachineInteractionManager? handler) || handler.SoundMachine is not { } soundMachine)
			{
				return 0;
			}

			return soundMachine.Id;
		}
	}
}
