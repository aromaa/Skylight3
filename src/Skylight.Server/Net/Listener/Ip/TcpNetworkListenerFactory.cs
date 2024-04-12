using Microsoft.Extensions.DependencyInjection;
using Skylight.API.Net.EndPoint;
using Skylight.API.Net.Listener;

namespace Skylight.Server.Net.Listener.Ip;

internal sealed class TcpNetworkListenerFactory(IServiceProvider serviceProvider) : INetworkListenerFactory
{
	private readonly IServiceProvider serviceProvider = serviceProvider;

	public bool CanHandle(INetworkEndPoint endPoint) => endPoint is IIpNetworkEndPoint;

	public INetworkListener CreateListener(INetworkEndPoint endPoint)
	{
		return ActivatorUtilities.CreateInstance<TcpNetworkListener>(this.serviceProvider, [((IIpNetworkEndPoint)endPoint).IpEndPoint]);
	}
}
