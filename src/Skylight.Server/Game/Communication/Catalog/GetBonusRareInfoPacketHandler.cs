using Net.Communication.Attributes;
using Skylight.API.Game.Users;
using Skylight.Protocol.Packets.Incoming.Catalog;
using Skylight.Protocol.Packets.Manager;
using Skylight.Protocol.Packets.Outgoing.Catalog;

namespace Skylight.Server.Game.Communication.Catalog;

[PacketManagerRegister(typeof(AbstractGamePacketManager))]
internal sealed class GetBonusRareInfoPacketHandler<T> : UserPacketHandler<T>
	where T : IGetBonusRareInfoIncomingPacket
{
	internal override void Handle(IUser user, in T packet)
	{
		user.SendAsync(new BonusRareInfoOutgoingPacket("bonusrare16_1_1", 6757, 120, 69));
	}
}
