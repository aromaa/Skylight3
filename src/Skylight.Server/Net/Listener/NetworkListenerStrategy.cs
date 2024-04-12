using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Skylight.API.Net.EndPoint;
using Skylight.API.Net.Listener;

namespace Skylight.Server.Net.Listener;

internal sealed class NetworkListenerStrategy(IServiceProvider serviceProvider, IEnumerable<INetworkListenerFactory> factories) : INetworkListenerStrategy
{
	private readonly IServiceProvider serviceProvider = serviceProvider;

	private readonly List<INetworkListenerFactory> factories = [.. factories];

	public bool TryCreateListener(INetworkEndPoint endPoint, [NotNullWhen(true)] out INetworkListener? listener)
	{
		foreach (INetworkListenerFactory factory in this.factories)
		{
			if (factory.CanHandle(endPoint))
			{
				listener = factory.CreateListener(endPoint);
				return true;
			}
		}

		listener = null;
		return false;
	}

	public void Register<T>()
		where T : INetworkListenerFactory => this.factories.Add(ActivatorUtilities.CreateInstance<T>(this.serviceProvider, []));
}
