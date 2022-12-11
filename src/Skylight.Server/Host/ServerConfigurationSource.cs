using Microsoft.Extensions.Configuration;

namespace Skylight.Server.Host;

internal sealed class ServerConfigurationSource : IConfigurationSource
{
	private readonly string? connectionString;

	internal ServerConfigurationSource(string? connectionString)
	{
		this.connectionString = connectionString;
	}

	public IConfigurationProvider Build(IConfigurationBuilder builder)
	{
		return new ServerConfigurationProvider(this.connectionString);
	}
}
