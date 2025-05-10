using System.Buffers;
using System.Buffers.Text;
using Net.Communication.Attributes;
using Skylight.API.Game.Clients;
using Skylight.Protocol.Packets.Incoming.Handshake;
using Skylight.Protocol.Packets.Manager;
using Skylight.Protocol.Packets.Outgoing.Handshake;

namespace Skylight.Server.Game.Communication.Handshake;

[PacketManagerRegister(typeof(IGamePacketManager))]
internal sealed class VersionCheckPacketHandler<T> : ClientPacketHandler<T>
	where T : IVersionCheckIncomingPacket
{
	internal override void Handle(IClient client, in T packet)
	{
		if (!Utf8Parser.TryParse(packet.VersionId.ToArray(), out int versionId, out _))
		{
			client.SendAsync(new CompleteDiffieHandshakeOutgoingPacket(string.Empty, false));
		}
	}
}
