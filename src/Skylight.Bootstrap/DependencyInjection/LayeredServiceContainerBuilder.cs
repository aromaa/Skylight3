using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace Skylight.Bootstrap.DependencyInjection;

internal class LayeredServiceContainerBuilder
{
	private readonly Dictionary<ServiceLayer, LayerData> layers;

	internal LayeredServiceContainerBuilder(IServiceCollection bootstrapLayer)
	{
		this.layers = new Dictionary<ServiceLayer, LayerData>
		{
			{ ServiceLayer.Bootstrap, new LayerData(bootstrapLayer) },
			{ ServiceLayer.Platform, new LayerData(new ServiceCollection()) }
		};
	}

	internal void Configure(ServiceLayer layer, Action<IServiceCollection> action) => action(this.layers[layer].Services);

	internal void Configure(ServiceLayer layer, Action<IServiceCollection> action, Action<IServiceProvider> configuration)
	{
		this.Configure(layer, action);

		this.layers[layer].Configurations.Add(configuration);
	}

	internal AutofacServiceProvider BuildLayer(ServiceLayer layer, Func<IServiceCollection, AutofacServiceProvider> factory)
	{
		LayerData layerData = this.layers[layer];
		AutofacServiceProvider provider = factory(layerData.Services);

		foreach (Action<IServiceProvider> configuration in layerData.Configurations)
		{
			configuration(provider);
		}

		return provider;
	}

	private class LayerData(IServiceCollection services)
	{
		internal IServiceCollection Services { get; } = services;

		internal List<Action<IServiceProvider>> Configurations { get; } = [];
	}
}
