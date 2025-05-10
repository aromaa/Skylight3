using System.Buffers;
using System.Buffers.Text;
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
		if (!Utf8Parser.TryParse(packet.VersionId.ToArray(), out int versionId, out _))
		{
			IPipelineHandlerContext? context = client.Socket.Pipeline.Context;
			while (context is not null)
			{
				if (context.Handler is FusePacketHeaderHandler<string> stringHandler)
				{
					client.SendAsync(new CompleteDiffieHandshakeOutgoingPacket(string.Empty, false));
				}
				else if (context.Handler is FusePacketHeaderHandler<int> intHandler)
				{
					intHandler.SetSecretKey();

					client.SendAsync(new CompleteDiffieHandshakeOutgoingPacket(string.Empty, false));

					break;
				}

				context = context.Next;
			}
		}
	}
}
