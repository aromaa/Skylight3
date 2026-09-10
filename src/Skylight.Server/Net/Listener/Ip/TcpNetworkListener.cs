using System.Net;
using Microsoft.Extensions.Logging;
using Net.Sockets.Listener;
using Skylight.API.Net.Connection;
using Skylight.API.Net.Listener;
using Skylight.Server.Extensions;
using Skylight.Server.Net.Communication;

namespace Skylight.Server.Net.Listener.Ip;

internal sealed class TcpNetworkListener(IServiceProvider serviceProvider, ILogger<TcpNetworkListener> logger, INetworkConnectionHandler connectionHandler, PacketManagerCache packetManagerCache, Uri endPoint) : INetworkListener
{
	private readonly IServiceProvider serviceProvider = serviceProvider;
	private readonly ILogger<TcpNetworkListener> logger = logger;

	private readonly INetworkConnectionHandler connectionHandler = connectionHandler;

	private readonly PacketManagerCache packetManagerCache = packetManagerCache;

	private readonly Uri endPoint = endPoint;

	public void Start(NetworkListenerConfiguration configuration)
	{
		if (!this.packetManagerCache.TryCreatePacketManager(configuration.Revision!, out _))
		{
			this.logger.LogWarning($"Did not find a packet manager for revision {configuration.Revision}.");

			return;
		}

		IPEndPoint ipEndPoint = IPEndPoint.Parse(this.endPoint.Authority);

		this.logger.LogInformation($"Listening on {ipEndPoint}");

		IListener.CreateTcpListener(ipEndPoint, socket =>
		{
			this.connectionHandler.Accept(socket, configuration.Encoding, configuration.Revision!, configuration.CryptoPrime, configuration.CryptoGenerator, configuration.CryptoKey, configuration.CryptoPremix, configuration.DecodePremix);
		}, this.serviceProvider);
	}

	public void Stop()
	{
	}

	public void Dispose() => this.DisposeAsync().Wait();
	public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}
