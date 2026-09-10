using System.Diagnostics.CodeAnalysis;
using Skylight.API.Net.Listener;

namespace Skylight.Server.Net.Listener;

internal sealed class NetworkListenerStrategy(IEnumerable<INetworkListenerFactory> factories) : INetworkListenerStrategy
{
	private readonly List<INetworkListenerFactory> factories = [.. factories];

	public bool TryCreateListener(Uri endPoint, [NotNullWhen(true)] out INetworkListener? listener)
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
}
