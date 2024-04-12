using Skylight.API.Net.EndPoint;

namespace Skylight.Server.Net.EndPoint;

internal sealed class UriNetworkEndPoint(Uri uri) : IUriNetworkEndPoint
{
	public Uri UriEndPoint { get; } = uri;
}
