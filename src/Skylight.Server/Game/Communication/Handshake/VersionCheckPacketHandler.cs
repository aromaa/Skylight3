using Net.Communication.Attributes;
using Net.Sockets.Pipeline.Handler;
using Skylight.API.Game.Clients;
using Skylight.Protocol.Packets.Incoming.Handshake;
using Skylight.Protocol.Packets.Manager;
using Skylight.Protocol.Packets.Outgoing.Handshake;
using Skylight.Server.Net.Handlers;

namespace Skylight.Server.Game.Communication.Handshake;

[PacketManagerRegister(typeof(IGamePacketManager))]
internal sealed class VersionCheckPacketHandler<T> : ClientPacketHandler<T>
	where T : IVersionCheckIncomingPacket
{
	internal override void Handle(IClient client, in T packet)
	{
		IPipelineHandlerContext? context = client.Socket.Pipeline.Context;
		while (context is not null)
		{
			if (context.Handler is FusePacketHeaderHandler<string> stringHandler)
			{
				client.SendAsync(new CompleteDiffieHandshakeOutgoingPacket(string.Empty, false));

				break;
			}
			else if (context.Handler is FusePacketHeaderHandler<int> intHandler)
			{
				intHandler.SetSecretKey();

				client.SendAsync(new CompleteDiffieHandshakeOutgoingPacket(string.Empty, false));

				break;
			}
			else if (context.Handler is Base64PacketHeaderHandler { CheckVersionBased: true } base64Handler)
			{
				base64Handler.SetSecretKey();

				client.SendAsync(new CompleteDiffieHandshakeOutgoingPacket(string.Empty, false));

				break;
			}

			context = context.Next;
		}
	}
}
