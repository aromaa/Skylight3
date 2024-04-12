using System.Net;
using Skylight.API.Net.EndPoint;

namespace Skylight.Server.Net.EndPoint;

internal sealed class IpNetworkEndPoint(IPEndPoint ipEndPoint) : IIpNetworkEndPoint
{
	public IPEndPoint IpEndPoint { get; } = ipEndPoint;
}
