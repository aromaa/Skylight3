using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Skylight.Server.Host;

internal sealed class ServerHostBuilder : IServerHostBuilder
{
	private readonly IHostBuilder builder;

	public ServerHostBuilder(IHostBuilder builder)
	{
		this.builder = builder;
	}

	public IServerHostBuilder ConfigureAppConfiguration(Action<HostBuilderContext, IConfigurationBuilder> configureDelegate)
	{
		this.builder.ConfigureAppConfiguration(configureDelegate);

		return this;
	}

	public IServerHostBuilder ConfigureServices(Action<HostBuilderContext, IServiceCollection> configureDelegate)
	{
		this.builder.ConfigureServices(configureDelegate);

		return this;
	}
}
