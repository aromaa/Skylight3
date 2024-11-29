using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace Skylight.Bootstrap.DependencyInjection;

internal class LayeredServiceProviderFactory : IServiceProviderFactory<LayeredServiceContainerBuilder>
{
	public LayeredServiceContainerBuilder CreateBuilder(IServiceCollection services) => new(services);

	public IServiceProvider CreateServiceProvider(LayeredServiceContainerBuilder containerBuilder)
	{
		AutofacServiceProvider bootstrapLayer = containerBuilder.BuildLayer(ServiceLayer.Bootstrap, static services =>
		{
			ContainerBuilder builder = new();
			builder.Populate(services);

			return new AutofacServiceProvider(builder.Build());
		});

		AutofacServiceProvider platformLayer = containerBuilder.BuildLayer(ServiceLayer.Platform, services =>
		{
			ILifetimeScope platformLayer = bootstrapLayer.LifetimeScope.BeginLifetimeScope(builder => builder.Populate(services));

			return new AutofacServiceProvider(platformLayer);
		});

		return platformLayer;
	}
}
