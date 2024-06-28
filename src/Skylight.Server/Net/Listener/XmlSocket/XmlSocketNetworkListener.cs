using System.Net;
using Microsoft.Extensions.Logging;
using Net.Sockets;
using Net.Sockets.Listener;
using Skylight.API.Net.Listener;
using Skylight.Server.Extensions;
using Skylight.Server.Net.Handlers;

namespace Skylight.Server.Net.Listener.XmlSocket;

internal sealed class XmlSocketNetworkListener(ILogger<XmlSocketNetworkListener> logger, Uri endPoint) : INetworkListener
{
	private readonly ILogger<XmlSocketNetworkListener> logger = logger;

	private readonly Uri endPoint = endPoint;

	public void Start(NetworkListenerConfiguration configuration)
	{
		IPEndPoint ipEndPoint = IPEndPoint.Parse(this.endPoint.Authority);

		this.logger.LogInformation($"Listening on {ipEndPoint}");

		IListener.CreateTcpListener(ipEndPoint, socket =>
		{
			socket.Pipeline.AddHandlerFirst(FlashSocketPolicyRequestHandler.Instance);

			Task.Delay(TimeSpan.FromSeconds(5)).ContinueWith(static (_, state) => ((ISocket)state!).Disconnect("Timeout"), socket);
		});
	}

	public void Stop()
	{
	}

	public void Dispose() => this.DisposeAsync().Wait();
	public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}
