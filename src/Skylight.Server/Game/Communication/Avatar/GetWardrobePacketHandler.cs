using Microsoft.EntityFrameworkCore;
using Net.Communication.Attributes;
using Skylight.API.Game.Users;
using Skylight.Infrastructure;
using Skylight.Protocol.Packets.Data.Avatar;
using Skylight.Protocol.Packets.Incoming.Avatar;
using Skylight.Protocol.Packets.Manager;
using Skylight.Protocol.Packets.Outgoing.Avatar;

namespace Skylight.Server.Game.Communication.Avatar;

[PacketManagerRegister(typeof(IGamePacketManager))]
internal sealed class GetWardrobePacketHandler<T>(IDbContextFactory<SkylightContext> dbContextFactory) : UserPacketHandler<T>
	where T : IGetWardrobeIncomingPacket
{
	private readonly IDbContextFactory<SkylightContext> dbContextFactory = dbContextFactory;

	internal override void Handle(IUser user, in T packet)
	{
		user.Client.ScheduleTask(async client =>
		{
			await using SkylightContext dbContext = await this.dbContextFactory.CreateDbContextAsync().ConfigureAwait(false);

			List<WardrobeSlotData> wardrobe = await dbContext.UserWardrobe
				.Where(e => e.UserId == user.Profile.Id)
				.Select(s => new WardrobeSlotData(s.SlotId, s.Gender, s.Figure))
				.ToListAsync().ConfigureAwait(false);

			client.SendAsync(new WardrobeOutgoingPacket(wardrobe));
		});
	}
}
