using Microsoft.Extensions.Hosting;
using Skylight.API.Server;

namespace Skylight.Server.Host;

internal sealed class ServerHostService : IHostedService
{
	private readonly IServer server;

	public ServerHostService(IServer server)
	{
		this.server = server;
	}

	public async Task StartAsync(CancellationToken cancellationToken)
	{
		await this.server.StartAsync(cancellationToken).ConfigureAwait(false);
	}

	public async Task StopAsync(CancellationToken cancellationToken)
	{
		await this.server.StopAsync(cancellationToken).ConfigureAwait(false);
	}
}
