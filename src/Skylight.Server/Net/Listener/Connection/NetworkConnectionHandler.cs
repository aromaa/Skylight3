using System.Numerics;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Net.Communication.Manager;
using Net.Metadata;
using Net.Sockets;
using Skylight.API.Game.Clients;
using Skylight.API.Net.Connection;
using Skylight.API.Registry;
using Skylight.Protocol.Packets.Manager;
using Skylight.Protocol.Packets.Outgoing.Handshake;
using Skylight.Server.Game.Clients;
using Skylight.Server.Net.Communication;
using Skylight.Server.Net.Handlers;

namespace Skylight.Server.Net.Listener.Connection;

internal sealed class NetworkConnectionHandler(IServiceProvider serviceProvider, IRegistryHolder registryHolder, ILogger<NetworkConnectionHandler> logger, IClientManager clientManager, PacketManagerCache packetManagerCache) : INetworkConnectionHandler
{
	public static readonly MetadataKey<Client> GameClientMetadataKey = MetadataKey<Client>.Create("GameClient");

	private readonly IServiceProvider serviceProvider = serviceProvider;
	private readonly IRegistryHolder registryHolder = registryHolder;
	private readonly ILogger<NetworkConnectionHandler> logger = logger;

	private readonly IClientManager clientManager = clientManager;
	private readonly PacketManagerCache packetManagerCache = packetManagerCache;

	public void Accept(ISocket socket, Encoding encoding, string revision, string? cryptoPrime = null, string? cryptoGenerator = null, string? cryptoKey = null, string? cryptoPremix = null)
	{
		if (!this.packetManagerCache.TryCreatePacketManager(revision, out Func<IGamePacketManager>? packetManagerGetter))
		{
			this.logger.LogWarning($"Did not find a packet manager for revision {revision}.");

			return;
		}

		Client client = new(this.registryHolder, socket, encoding);
		if (!this.clientManager.TryAccept(client))
		{
			socket.Disconnect();

			return;
		}

		socket.Metadata.Set(NetworkConnectionHandler.GameClientMetadataKey, client);

		socket.Pipeline.AddHandlerFirst(new LeftOverHandler());

		IGamePacketManager packetManager = packetManagerGetter();
		if (packetManager.Modern)
		{
			socket.Pipeline.AddHandlerFirst(new HotSwapPacketHandler(packetManagerGetter));
			socket.Pipeline.AddHandlerFirst(FlashSocketPolicyRequestHandler.Instance);
		}
		else
		{
			if (packetManager.Fuse)
			{
				//TODO: Release 5 is quite special in many ways...
				if (((PacketManager<string>)packetManagerGetter()).TryGetComposer(typeof(AuthenticationOKOutgoingPacket), out _, out _))
				{
					socket.Pipeline.AddHandlerFirst(new FusePacketHeaderHandler<int>(packetManagerGetter, cryptoKey!));
				}
				else
				{
					socket.Pipeline.AddHandlerFirst(new FusePacketHeaderHandler<string>(packetManagerGetter, cryptoKey!));
				}
			}
			else
			{
				socket.Pipeline.AddHandlerFirst(new Base64PacketHeaderHandler(this.serviceProvider.GetRequiredService<ILogger<Base64PacketHeaderHandler>>(), packetManagerGetter, BigInteger.Parse(cryptoPrime ?? "0"), BigInteger.Parse(cryptoGenerator ?? "0"), cryptoKey!, cryptoPremix!));
			}

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
