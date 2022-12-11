using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Skylight.Server.Host;

public interface IServerHostBuilder
{
	public IServerHostBuilder ConfigureAppConfiguration(Action<HostBuilderContext, IConfigurationBuilder> configureDelegate);
	public IServerHostBuilder ConfigureServices(Action<HostBuilderContext, IServiceCollection> configureDelegate);
}
