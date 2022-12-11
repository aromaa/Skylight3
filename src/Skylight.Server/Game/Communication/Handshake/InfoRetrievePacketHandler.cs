using Net.Communication.Attributes;
using Skylight.API.Game.Users;
using Skylight.Protocol.Packets.Incoming.Handshake;
using Skylight.Protocol.Packets.Manager;
using Skylight.Protocol.Packets.Outgoing.Handshake;

namespace Skylight.Server.Game.Communication.Handshake;

[PacketManagerRegister(typeof(AbstractGamePacketManager))]
internal sealed class InfoRetrievePacketHandler<T> : UserPacketHandler<T>
	where T : IInfoRetrieveIncomingPacket
{
	internal override void Handle(IUser user, in T packet)
	{
		user.SendAsync(new UserObjectOutgoingPacket
		{
			UserId = user.Profile.Id,
			Username = user.Profile.Username,
			Figure = user.Profile.Figure,
			Gender = user.Profile.Gender,
			CustomData = string.Empty,
			Tickets = 0,
			SwimSuit = string.Empty,
			Film = 0,
			RealName = string.Empty,
			DirectMail = false,
			RespectTotal = 0,
			RespectLeft = 0,
			PerRespectLeft = 0,
			StreamPublishingAllowed = false,
			LastAccessDate = user.Profile.LastOnline,
			NameChangeAllowed = true,
			AccountSafetyLocked = false
		});
	}
}
