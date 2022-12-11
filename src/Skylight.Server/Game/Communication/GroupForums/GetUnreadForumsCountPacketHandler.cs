using Net.Communication.Attributes;
using Skylight.API.Game.Users;
using Skylight.Protocol.Packets.Incoming.GroupForums;
using Skylight.Protocol.Packets.Manager;

namespace Skylight.Server.Game.Communication.GroupForums;

[PacketManagerRegister(typeof(AbstractGamePacketManager))]
internal sealed class GetUnreadForumsCountPacketHandler<T> : UserPacketHandler<T>
	where T : IGetUnreadForumsCountIncomingPacket
{
	internal override void Handle(IUser user, in T packet)
	{
	}
}
