using System.Runtime.InteropServices;
using Microsoft.EntityFrameworkCore;
using Net.Communication.Attributes;
using Skylight.API.Game.Clients;
using Skylight.API.Game.Users;
using Skylight.Domain.Rooms.Sound;
using Skylight.Infrastructure;
using Skylight.Protocol.Packets.Incoming.Sound;
using Skylight.Protocol.Packets.Manager;

namespace Skylight.Server.Game.Communication.Sound;

[PacketManagerRegister(typeof(AbstractGamePacketManager))]
internal sealed class DeleteSongPacketHandler<T> : UserPacketHandler<T>
	where T : IDeleteSongIncomingPacket
{
	private readonly IDbContextFactory<SkylightContext> dbContextFactory;

	public DeleteSongPacketHandler(IDbContextFactory<SkylightContext> dbContextFactory)
	{
		this.dbContextFactory = dbContextFactory;
	}

	internal override void Handle(IUser user, in T packet)
	{
		user.Client.ScheduleTask(new DeleteSongTask
		{
			DbContextFactory = this.dbContextFactory,

			SongId = packet.SongId
		});
	}

	[StructLayout(LayoutKind.Auto)]
	private readonly struct DeleteSongTask : IClientTask
	{
		internal IDbContextFactory<SkylightContext> DbContextFactory { get; init; }

		internal int SongId { get; init; }

		public async Task ExecuteAsync(IClient client)
		{
			await using SkylightContext dbContext = await this.DbContextFactory.CreateDbContextAsync().ConfigureAwait(false);

			dbContext.Remove(new SongEntity
			{
				Id = this.SongId
			});

			await dbContext.SaveChangesAsync().ConfigureAwait(false);
		}
	}
}
