using Net.Communication.Attributes;
using Skylight.API.Game.Clients;
using Skylight.Protocol.Packets.Incoming.Handshake;
using Skylight.Protocol.Packets.Manager;
using Skylight.Protocol.Packets.Outgoing.Handshake;

namespace Skylight.Server.Game.Communication.Handshake;

[PacketManagerRegister(typeof(IGamePacketManager))]
internal sealed class GetSessionParametersPacketHandler<T> : ClientPacketHandler<T>
	where T : IGetSessionParametersIncomingPacket
{
	internal override void Handle(IClient client, in T packet)
	{
		client.SendAsync(new SessionParametersOutgoingPacket());
	}
}
