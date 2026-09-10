using Microsoft.Extensions.DependencyInjection;
using Skylight.API.Net.Listener;

namespace Skylight.Plugin.WebSockets;

public sealed class WebSocketNetworkListenerFactory(IServiceProvider serviceProvider) : INetworkListenerFactory
{
	private readonly IServiceProvider serviceProvider = serviceProvider;

	public bool CanHandle(Uri endPoint) => endPoint.Scheme is "ws" or "wss";

	public INetworkListener CreateListener(Uri endPoint)
	{
		return ActivatorUtilities.CreateInstance<WebSocketNetworkListener>(this.serviceProvider, [endPoint]);
	}
}
