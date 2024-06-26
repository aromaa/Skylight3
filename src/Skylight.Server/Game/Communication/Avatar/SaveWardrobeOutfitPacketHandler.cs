using System.Text;
using Microsoft.EntityFrameworkCore;
using Net.Communication.Attributes;
using Skylight.API.Game.Users;
using Skylight.Domain.Users;
using Skylight.Infrastructure;
using Skylight.Protocol.Packets.Incoming.Avatar;
using Skylight.Protocol.Packets.Manager;

namespace Skylight.Server.Game.Communication.Avatar;

[PacketManagerRegister(typeof(AbstractGamePacketManager))]
internal sealed class SaveWardrobeOutfitPacketHandler<T>(IDbContextFactory<SkylightContext> dbContextFactory) : UserPacketHandler<T>
	where T : ISaveWardrobeOutfitIncomingPacket
{
	private readonly IDbContextFactory<SkylightContext> dbContextFactory = dbContextFactory;

	internal override void Handle(IUser user, in T packet)
	{
		int slotId = packet.SlotId;
		if (slotId is <= 0 or > 10)
		{
			return;
		}

		string figure = Encoding.UTF8.GetString(packet.Figure);
		string gender = Encoding.UTF8.GetString(packet.Gender);

		user.Client.ScheduleTask(async _ =>
		{
			await using SkylightContext dbContext = await this.dbContextFactory.CreateDbContextAsync().ConfigureAwait(false);

			await dbContext.Upsert(new UserWardrobeSlotEntity
			{
				UserId = user.Profile.Id,
				SlotId = slotId,
				Gender = gender,
				Figure = figure
			}).WhenMatched((_, u) => new UserWardrobeSlotEntity
			{
				Gender = u.Gender,
				Figure = u.Figure
			}).RunAsync().ConfigureAwait(false);
		});
	}
}
