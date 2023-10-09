using System.Runtime.InteropServices;
using Net.Communication.Attributes;
using Skylight.API.Game.Clients;
using Skylight.API.Game.Users;
using Skylight.Protocol.Packets.Data.Users;
using Skylight.Protocol.Packets.Incoming.Users;
using Skylight.Protocol.Packets.Manager;
using Skylight.Protocol.Packets.Outgoing.Users;

namespace Skylight.Server.Game.Communication.Users;

[PacketManagerRegister(typeof(AbstractGamePacketManager))]
internal sealed class GetExtendedProfilePacketHandler<T> : UserPacketHandler<T>
	where T : IGetExtendedProfileIncomingPacket
{
	private readonly IUserManager userManager;

	public GetExtendedProfilePacketHandler(IUserManager userManager)
	{
		this.userManager = userManager;
	}

	internal override void Handle(IUser user, in T packet)
	{
		user.Client.ScheduleTask(new GetGuestRoomTask
		{
			UserManager = this.userManager,

			UserId = packet.UserId,
		});
	}

	[StructLayout(LayoutKind.Auto)]
	private readonly struct GetGuestRoomTask : IClientTask
	{
		internal IUserManager UserManager { get; init; }

		internal int UserId { get; init; }

		public async Task ExecuteAsync(IClient client)
		{
			IUserProfile? profile = await this.UserManager.GetUserProfileAsync(this.UserId).ConfigureAwait(false);
			if (profile is null)
			{
				return;
			}

			client.SendAsync(new ExtendedProfileOutgoingPacket(new ExtendedProfileData(this.UserId, profile!.Username, profile!.Figure, profile!.Motto, "08-03-2001", 666, 0, false, false, true, new List<string>(), (int)(DateTimeOffset.Now - profile!.LastOnline).TotalSeconds, true, false, 69, 8, 420, true, false)));
		}
	}
}
