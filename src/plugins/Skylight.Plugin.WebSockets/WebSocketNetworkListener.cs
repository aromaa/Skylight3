using Microsoft.Extensions.Logging;
using Net.Sockets.Listener;
using Skylight.API.Net.Connection;
using Skylight.API.Net.Listener;

namespace Skylight.Plugin.WebSockets;

internal sealed class WebSocketNetworkListener(IServiceProvider serviceProvider, ILogger<WebSocketNetworkListener> logger, INetworkConnectionHandler connectionHandler, Uri endPoint) : INetworkListener
{
	private readonly IServiceProvider serviceProvider = serviceProvider;
	private readonly ILogger<WebSocketNetworkListener> logger = logger;

	private readonly INetworkConnectionHandler connectionHandler = connectionHandler;

	private readonly Uri endPoint = endPoint;

	public void Start(NetworkListenerConfiguration configuration)
	{
		this.logger.LogInformation($"Listening on {this.endPoint}");

		IListener.CreateWebSocketListener(this.endPoint, socket =>
		{
			this.connectionHandler.Accept(socket, configuration!.Revision!, configuration.CryptoPrime, configuration.CryptoGenerator, configuration.CryptoKey, configuration.CryptoPremix);
		}, this.serviceProvider);
	}

	public void Stop()
	{
	}

	public void Dispose() => this.DisposeAsync().AsTask().GetAwaiter().GetResult();
	public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}
