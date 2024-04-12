using Skylight.API.Net.Connection;
using Skylight.API.Net.EndPoint;
using Skylight.API.Net.Listener;

namespace Skylight.Plugin.WebSockets;

public sealed class WebSocketNetworkListenerFactory(INetworkConnectionHandler connectionHandler) : INetworkListenerFactory
{
	private readonly INetworkConnectionHandler connectionHandler = connectionHandler;

	public bool CanHandle(INetworkEndPoint endPoint) => endPoint is IUriNetworkEndPoint { UriEndPoint.Scheme: "ws" or "wss" };

	public INetworkListener CreateListener(INetworkEndPoint endPoint)
	{
		return new WebSocketNetworkListener(this.connectionHandler, ((IUriNetworkEndPoint)endPoint).UriEndPoint);
	}
}
