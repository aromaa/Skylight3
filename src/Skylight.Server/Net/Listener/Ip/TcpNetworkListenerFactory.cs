using Microsoft.Extensions.DependencyInjection;
using Skylight.API.Net.Listener;

namespace Skylight.Server.Net.Listener.Ip;

internal sealed class TcpNetworkListenerFactory(IServiceProvider serviceProvider) : INetworkListenerFactory
{
	private readonly IServiceProvider serviceProvider = serviceProvider;

	public bool CanHandle(Uri endPoint) => endPoint.Scheme == "tcp";

	public INetworkListener CreateListener(Uri endPoint)
	{
		return ActivatorUtilities.CreateInstance<TcpNetworkListener>(this.serviceProvider, [endPoint]);
	}
}
