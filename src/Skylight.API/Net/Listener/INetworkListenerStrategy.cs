using System.Diagnostics.CodeAnalysis;

namespace Skylight.API.Net.Listener;

public interface INetworkListenerStrategy
{
	public bool TryCreateListener(Uri endPoint, [NotNullWhen(true)] out INetworkListener? listener);
}
