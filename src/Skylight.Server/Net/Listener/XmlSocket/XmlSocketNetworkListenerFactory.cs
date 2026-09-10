using Microsoft.Extensions.DependencyInjection;
using Skylight.API.Net.Listener;

namespace Skylight.Server.Net.Listener.XmlSocket;

internal sealed class XmlSocketNetworkListenerFactory(IServiceProvider serviceProvider) : INetworkListenerFactory
{
	private readonly IServiceProvider serviceProvider = serviceProvider;

	public bool CanHandle(Uri endPoint) => endPoint.Scheme == "xmlsocket";

	public INetworkListener CreateListener(Uri endPoint)
	{
		return ActivatorUtilities.CreateInstance<XmlSocketNetworkListener>(this.serviceProvider, [endPoint]);
	}
}
