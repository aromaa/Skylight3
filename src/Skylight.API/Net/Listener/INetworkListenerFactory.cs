namespace Skylight.API.Net.Listener;

public interface INetworkListenerFactory
{
	public bool CanHandle(Uri endPoint);

	public INetworkListener CreateListener(Uri endPoint);
}
