using System.Net;
using Microsoft.Extensions.DependencyInjection;
using Skylight.API.Net.EndPoint;
using Skylight.API.Net.Listener;

namespace Skylight.Server.Net.Listener.XmlSocket;

internal sealed class XmlSocketNetworkListenerFactory(IServiceProvider serviceProvider) : INetworkListenerFactory
{
	private readonly IServiceProvider serviceProvider = serviceProvider;

	public bool CanHandle(INetworkEndPoint endPoint) => endPoint is IUriNetworkEndPoint { UriEndPoint.Scheme: "xmlsocket" } uri && IPEndPoint.TryParse(uri.UriEndPoint.Authority, out _);

	public INetworkListener CreateListener(INetworkEndPoint endPoint)
	{
		return ActivatorUtilities.CreateInstance<XmlSocketNetworkListener>(this.serviceProvider, [((IUriNetworkEndPoint)endPoint).UriEndPoint]);
	}
}
