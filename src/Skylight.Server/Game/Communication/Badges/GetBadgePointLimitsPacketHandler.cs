using Net.Communication.Attributes;
using Skylight.API.Game.Users;
using Skylight.Protocol.Packets.Incoming.Badges;
using Skylight.Protocol.Packets.Manager;

namespace Skylight.Server.Game.Communication.Badges;

[PacketManagerRegister(typeof(AbstractGamePacketManager))]
internal sealed class GetBadgePointLimitsPacketHandler<T> : UserPacketHandler<T>
	where T : IGetBadgePointLimitsIncomingPacket
{
	internal override void Handle(IUser user, in T packet)
	{
	}
}
