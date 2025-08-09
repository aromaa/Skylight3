using Microsoft.EntityFrameworkCore;
using Net.Communication.Attributes;
using Skylight.API.Game.Users;
using Skylight.Domain.Users;
using Skylight.Infrastructure;
using Skylight.Protocol.Packets.Incoming.Preferences;
using Skylight.Protocol.Packets.Manager;

namespace Skylight.Server.Game.Communication.Preferences;

[PacketManagerRegister(typeof(IGamePacketManager))]
internal sealed partial class SetSoundSettingsPacketHandler<T>(IDbContextFactory<SkylightContext> dbContextFactory) : UserPacketHandler<T>
	where T : ISetSoundSettingsIncomingPacket
{
	private readonly IDbContextFactory<SkylightContext> dbContextFactory = dbContextFactory;

	internal override void Handle(IUser user, in T packet)
	{
		if (packet.UiVolume < 0 || packet.UiVolume > 100 || packet.FurniVolume < 0 || packet.FurniVolume > 100 || packet.TraxVolume < 0 || packet.TraxVolume > 100)
		{
			return;
		}

		int uiVolume = packet.UiVolume;
		int furniVolume = packet.FurniVolume;
		int traxVolume = packet.TraxVolume;

		user.Client.ScheduleTask(async client =>
		{
			await using SkylightContext dbContext = await this.dbContextFactory.CreateDbContextAsync().ConfigureAwait(false);

			await dbContext.UserSettings.Upsert(new UserSettingsEntity
			{
				UserId = client.User!.Id,
				UiVolume = uiVolume,
				FurniVolume = furniVolume,
				TraxVolume = traxVolume,
			}).On(c => c.UserId)
			.WhenMatched((_, c) => new UserSettingsEntity
			{
				UiVolume = c.UiVolume,
				FurniVolume = c.FurniVolume,
				TraxVolume = c.TraxVolume,
			}).RunAsync().ConfigureAwait(false);

			client.User.Settings.UiVolume = uiVolume;
			client.User.Settings.FurniVolume = furniVolume;
			client.User.Settings.TraxVolume = traxVolume;

			await dbContext.SaveChangesAsync().ConfigureAwait(false);
		});
	}
}
