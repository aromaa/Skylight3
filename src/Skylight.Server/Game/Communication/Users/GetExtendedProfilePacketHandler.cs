using Net.Communication.Attributes;
using Skylight.API.Game.Users;
using Skylight.Protocol.Packets.Data.Users;
using Skylight.Protocol.Packets.Incoming.Users;
using Skylight.Protocol.Packets.Manager;
using Skylight.Protocol.Packets.Outgoing.Users;

namespace Skylight.Server.Game.Communication.Users;

[PacketManagerRegister(typeof(IGamePacketManager))]
internal sealed partial class GetExtendedProfilePacketHandler<T>(IUserManager userManager) : UserPacketHandler<T>
	where T : IGetExtendedProfileIncomingPacket
{
	private readonly IUserManager userManager = userManager;

	internal override void Handle(IUser user, in T packet)
	{
		int userId = packet.UserId;

		user.Client.ScheduleTask(async client =>
		{
			IUserProfile? profile = await this.userManager.GetUserProfileAsync(userId).ConfigureAwait(false);
			if (profile is null)
			{
				return;
			}

			client.SendAsync(new ExtendedProfileOutgoingPacket(new ExtendedProfileData(userId, profile.Username, profile.Figure, profile.Motto, "08-03-2001", 666, 0, false, false, true, [], (int)(DateTime.Now - profile.LastOnline).TotalSeconds, true, false, 69, 8, 420, true, false)));
		});
	}
}
