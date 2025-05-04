using Net.Communication.Attributes;
using Skylight.API.Game.Users;
using Skylight.Protocol.Packets.Incoming.Users;
using Skylight.Protocol.Packets.Manager;

namespace Skylight.Server.Game.Communication.Users;

[PacketManagerRegister(typeof(IGamePacketManager))]
internal sealed class GetMOTDPacketHandler<T> : UserPacketHandler<T>
	where T : IGetMOTDIncomingPacket
{
	internal override void Handle(IUser user, in T packet)
	{
	}
}
