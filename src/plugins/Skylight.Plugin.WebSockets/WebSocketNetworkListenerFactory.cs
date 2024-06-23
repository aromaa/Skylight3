using Microsoft.Extensions.DependencyInjection;
using Skylight.API.Net.EndPoint;
using Skylight.API.Net.Listener;

namespace Skylight.Plugin.WebSockets;

public sealed class WebSocketNetworkListenerFactory(IServiceProvider serviceProvider) : INetworkListenerFactory
{
	private readonly IServiceProvider serviceProvider = serviceProvider;

	public bool CanHandle(INetworkEndPoint endPoint) => endPoint is IUriNetworkEndPoint { UriEndPoint.Scheme: "ws" or "wss" };

	public INetworkListener CreateListener(INetworkEndPoint endPoint)
	{
		return ActivatorUtilities.CreateInstance<WebSocketNetworkListener>(this.serviceProvider, [((IUriNetworkEndPoint)endPoint).UriEndPoint]);
	}
}
