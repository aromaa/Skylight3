using Net.Communication.Attributes;
using Skylight.API.Game.Clients;
using Skylight.Protocol.Packets.Incoming.Handshake;
using Skylight.Protocol.Packets.Manager;

namespace Skylight.Server.Game.Communication.Handshake;

[PacketManagerRegister(typeof(AbstractGamePacketManager))]
internal sealed class ClientHelloPacketHandler<T> : ClientPacketHandler<T>
	where T : IClientHelloIncomingPacket
{
	internal override void Handle(IClient client, in T packet)
	{
	}
}
