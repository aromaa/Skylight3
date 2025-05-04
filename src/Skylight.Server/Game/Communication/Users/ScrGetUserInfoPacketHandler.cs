using System.Text;
using Net.Communication.Attributes;
using Skylight.API.Game.Users;
using Skylight.Protocol.Packets.Incoming.Users;
using Skylight.Protocol.Packets.Manager;
using Skylight.Protocol.Packets.Outgoing.Users;

namespace Skylight.Server.Game.Communication.Users;

[PacketManagerRegister(typeof(IGamePacketManager))]
internal sealed class ScrGetUserInfoPacketHandler<T> : UserPacketHandler<T>
	where T : IScrGetUserInfoIncomingPacket
{
	internal override void Handle(IUser user, in T packet)
	{
		user.SendAsync(new ScrSendUserInfoOutgoingPacket
		{
			ProductName = user.Client.Encoding.GetString(packet.ProductName),
			DaysToPeriodEnd = 0,
			MemberPeriods = 1,
			PeriodsSubscribedAhead = 1,
			ResponseType = 1,
			HasEverBeenMember = true,
			IsVIP = true,
			PastClubDays = 3,
			PastVipDays = 3,
			MinutesUntilExpiration = 1337,
			MinutesSinceLastModified = -1
		});
	}
}
