using Skylight.API.Registry;

namespace Skylight.API.Server;

public interface IServer : IRegistryHolder
{
	public Task StartAsync(CancellationToken cancellationToken);
	public Task StopAsync(CancellationToken cancellationToken);
}
