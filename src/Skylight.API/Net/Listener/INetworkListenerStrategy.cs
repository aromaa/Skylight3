using System.Diagnostics.CodeAnalysis;
using Skylight.API.Net.EndPoint;

namespace Skylight.API.Net.Listener;

public interface INetworkListenerStrategy
{
	public bool TryCreateListener(INetworkEndPoint endPoint, [NotNullWhen(true)] out INetworkListener? listener);

	//TODO
	public void Register<T>()
		where T : INetworkListenerFactory;
}
