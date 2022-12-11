using Net.Communication.Attributes;
using Skylight.API.Game.Users;
using Skylight.Protocol.Packets.Incoming.Advertisement;
using Skylight.Protocol.Packets.Manager;
using Skylight.Protocol.Packets.Outgoing.Advertisement;

namespace Skylight.Server.Game.Communication.Advertisement;

[PacketManagerRegister(typeof(AbstractGamePacketManager))]
internal sealed class GetInterstitialPacketHandler<T> : UserPacketHandler<T>
	where T : IGetInterstitialIncomingPacket
{
	internal override void Handle(IUser user, in T packet)
	{
		user.SendAsync(new InterstitialOutgoingPacket(false));
	}
}
