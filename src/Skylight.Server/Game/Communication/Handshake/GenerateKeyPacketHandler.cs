using Net.Communication.Attributes;
using Net.Sockets.Pipeline.Handler;
using Skylight.API.Game.Clients;
using Skylight.Protocol.Packets.Incoming.Handshake;
using Skylight.Protocol.Packets.Manager;
using Skylight.Protocol.Packets.Outgoing.Handshake;
using Skylight.Server.Net.Handlers;

namespace Skylight.Server.Game.Communication.Handshake;

[PacketManagerRegister(typeof(AbstractGamePacketManager))]
internal sealed class GenerateKeyPacketHandler<T> : ClientPacketHandler<T>
	where T : IGenerateKeyIncomingPacket
{
	internal override void Handle(IClient client, in T packet)
	{
		IPipelineHandlerContext? context = client.Socket.Pipeline.Context;
		while (context is not null)
		{
			if (context.Handler is Base64PacketHeaderHandler handler)
			{
				handler.SetSecretKey(20);

				break;
			}

			context = context.Next;
		}

		client.SendAsync(new CompleteDiffieHandshakeOutgoingPacket(string.Empty, false));
	}
}
