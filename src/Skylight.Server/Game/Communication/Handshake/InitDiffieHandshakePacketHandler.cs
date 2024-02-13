using System.Numerics;
using System.Security.Cryptography;
using Net.Communication.Attributes;
using Net.Sockets.Pipeline.Handler;
using Skylight.API.Game.Clients;
using Skylight.Protocol.Packets.Incoming.Handshake;
using Skylight.Protocol.Packets.Manager;
using Skylight.Protocol.Packets.Outgoing.Handshake;
using Skylight.Server.Net.Handlers;

namespace Skylight.Server.Game.Communication.Handshake;

[PacketManagerRegister(typeof(AbstractGamePacketManager))]
internal sealed class InitDiffieHandshakePacketHandler<T> : ClientPacketHandler<T>
	where T : IInitDiffieHandshakeIncomingPacket
{
	internal override void Handle(IClient client, in T packet)
	{
		IPipelineHandlerContext? context = client.Socket.Pipeline.Context;
		while (context is not null)
		{
			if (context.Handler is Base64PacketHeaderHandler handler)
			{
				Span<byte> tokenBytes = stackalloc byte[12];

				RandomNumberGenerator.Fill(tokenBytes);

				BigInteger token = new(tokenBytes, isUnsigned: true);

				handler.SetToken(token);

				client.SendAsync(new InitDiffieHandshakeOutgoingPacket(token.ToString("X"), true));

				return;
			}

			context = context.Next;
		}

		client.SendAsync(new InitDiffieHandshakeOutgoingPacket(string.Empty, false));
	}
}
