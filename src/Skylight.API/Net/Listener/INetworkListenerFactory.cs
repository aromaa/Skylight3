using Skylight.API.Net.EndPoint;

namespace Skylight.API.Net.Listener;

public interface INetworkListenerFactory
{
	public bool CanHandle(INetworkEndPoint endPoint);

	public INetworkListener CreateListener(INetworkEndPoint endPoint);
}
