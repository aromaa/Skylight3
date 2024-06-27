using System.Net;
using System.Text;
using Microsoft.Extensions.Logging;
using Net.Sockets.Listener;
using Skylight.API.Net.Connection;
using Skylight.API.Net.Listener;
using Skylight.Protocol.Packets.Manager;
using Skylight.Server.Extensions;
using Skylight.Server.Net.Communication;

namespace Skylight.Server.Net.Listener.Ip;

internal sealed class TcpNetworkListener(IServiceProvider serviceProvider, ILogger<TcpNetworkListener> logger, INetworkConnectionHandler connectionHandler, PacketManagerCache packetManagerCache, IPEndPoint endPoint) : INetworkListener
{
	private readonly IServiceProvider serviceProvider = serviceProvider;
	private readonly ILogger<TcpNetworkListener> logger = logger;

	private readonly INetworkConnectionHandler connectionHandler = connectionHandler;

	private readonly PacketManagerCache packetManagerCache = packetManagerCache;

	private readonly IPEndPoint endPoint = endPoint;

	public void Start(NetworkListenerConfiguration configuration)
	{
		if (!this.packetManagerCache.TryCreatePacketManager(configuration.Revision!, out Func<AbstractGamePacketManager>? packetManagerGetter))
		{
			this.logger.LogWarning($"Did not find a packet manager for revision {configuration.Revision}.");

			return;
		}

		this.logger.LogInformation($"Listening on {this.endPoint}");

		IListener.CreateTcpListener(this.endPoint, socket =>
		{
			this.connectionHandler.Accept(socket, configuration.Encoding, configuration.Revision!, configuration.CryptoPrime, configuration.CryptoGenerator, configuration.CryptoKey, configuration.CryptoPremix);
		}, this.serviceProvider);
	}

	public void Stop()
	{
	}

	public void Dispose() => this.DisposeAsync().Wait();
	public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}
