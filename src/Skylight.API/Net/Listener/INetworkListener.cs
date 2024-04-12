namespace Skylight.API.Net.Listener;

public interface INetworkListener : IAsyncDisposable, IDisposable
{
	public void Start(NetworkListenerConfiguration configuration);
	public void Stop();
}
