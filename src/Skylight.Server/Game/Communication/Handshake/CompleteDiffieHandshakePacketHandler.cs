using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using Net.Communication.Attributes;
using Net.Sockets.Pipeline.Handler;
using Skylight.API.Game.Clients;
using Skylight.Protocol.Packets.Incoming.Handshake;
using Skylight.Protocol.Packets.Manager;
using Skylight.Protocol.Packets.Outgoing.Handshake;
using Skylight.Server.Net.Handlers;

namespace Skylight.Server.Game.Communication.Handshake;

[PacketManagerRegister(typeof(AbstractGamePacketManager))]
internal sealed class CompleteDiffieHandshakePacketHandler<T> : ClientPacketHandler<T>
	where T : ICompleteDiffieHandshakeIncomingPacket
{
	internal override void Handle(IClient client, in T packet)
	{
		IPipelineHandlerContext? context = client.Socket.Pipeline.Context;
		while (context is not null)
		{
			if (context.Handler is Base64PacketHeaderHandler handler)
			{
				Span<byte> privateKeyBytes = stackalloc byte[12];

				do
				{
					RandomNumberGenerator.Fill(privateKeyBytes);
				}
				while (privateKeyBytes.SequenceEqual(new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }));

				BigInteger privateKey = new(privateKeyBytes, isUnsigned: true);
				BigInteger serverPublicKey = BigInteger.ModPow(handler.CryptoGenerator, privateKey, handler.CryptoPrime);

				BigInteger clientPublicKey = BigInteger.Parse(Encoding.UTF8.GetString(packet.PublicKey));
				BigInteger sharedKey = BigInteger.ModPow(clientPublicKey, privateKey, handler.CryptoPrime);

				handler.EnableEncryption(sharedKey, incomingOnly: false);

				client.SendAsync(new CompleteDiffieHandshakeOutgoingPacket(serverPublicKey.ToString(), false));

				return;
			}

			context = context.Next;
		}

		client.SendAsync(new CompleteDiffieHandshakeOutgoingPacket(string.Empty, false));
	}
}
