using Net.Communication.Attributes;
using Skylight.API.Game.Users;
using Skylight.Protocol.Packets.Incoming.Marketplace;
using Skylight.Protocol.Packets.Manager;
using Skylight.Protocol.Packets.Outgoing.Marketplace;

namespace Skylight.Server.Game.Communication.Marketplace;

[PacketManagerRegister(typeof(AbstractGamePacketManager))]
internal sealed class GetMarketplaceConfigurationPacketHandler<T> : UserPacketHandler<T>
	where T : IGetMarketplaceConfigurationIncomingPacket
{
	internal override void Handle(IUser user, in T packet)
	{
		user.SendAsync(new MarketplaceConfigurationOutgoingPacket(true, 1, 10, 5, 1, 100000, 48, 7, 2, 100000, 40000));
	}
}
