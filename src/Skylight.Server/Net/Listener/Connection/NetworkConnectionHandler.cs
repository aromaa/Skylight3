using System.Numerics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Net.Metadata;
using Net.Sockets;
using Skylight.API.Net.Connection;
using Skylight.Protocol.Packets.Manager;
using Skylight.Protocol.Packets.Outgoing.Handshake;
using Skylight.Server.Game.Clients;
using Skylight.Server.Net.Communication;
using Skylight.Server.Net.Handlers;

namespace Skylight.Server.Net.Listener.Connection;

internal sealed class NetworkConnectionHandler(IServiceProvider serviceProvider, ILogger<NetworkConnectionHandler> logger, PacketManagerCache packetManagerCache) : INetworkConnectionHandler
{
	public static readonly MetadataKey<Client> GameClientMetadataKey = MetadataKey<Client>.Create("GameClient");

	private readonly IServiceProvider serviceProvider = serviceProvider;
	private readonly ILogger<NetworkConnectionHandler> logger = logger;

	private readonly PacketManagerCache packetManagerCache = packetManagerCache;

	public void Accept(ISocket socket, string revision, string? cryptoPrime = null, string? cryptoGenerator = null, string? cryptoKey = null, string? cryptoPremix = null)
	{
		if (!this.packetManagerCache.TryCreatePacketManager(revision, out Func<AbstractGamePacketManager>? packetManagerGetter))
		{
			this.logger.LogWarning($"Did not find a packet manager for revision {revision}.");

			return;
		}

		socket.Metadata.Set(NetworkConnectionHandler.GameClientMetadataKey, new Client(socket));

		socket.Pipeline.AddHandlerFirst(new LeftOverHandler());

		AbstractGamePacketManager packetManager = packetManagerGetter();
		if (packetManager.Modern)
		{
			socket.Pipeline.AddHandlerFirst(new HotSwapPacketHandler(packetManagerGetter));
			socket.Pipeline.AddHandlerFirst(FlashSocketPolicyRequestHandler.Instance);
		}
		else
		{
			socket.Pipeline.AddHandlerFirst(new Base64PacketHeaderHandler(this.serviceProvider.GetRequiredService<ILogger<Base64PacketHeaderHandler>>(), packetManager, BigInteger.Parse(cryptoPrime ?? "0"), BigInteger.Parse(cryptoGenerator ?? "0"), cryptoKey!, cryptoPremix!));

			Task.Run(async () =>
			{
				await Task.Yield();

				try
				{
					_ = socket.Pipeline.Socket.SendAsync(new ServerHelloOutgoingPacket());
				}
				catch
				{
					//TODO: Fix
				}
			});
		}
	}
}
