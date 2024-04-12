using System.Net;

namespace Skylight.API.Net.EndPoint;

public interface IIpNetworkEndPoint : INetworkEndPoint
{
	public IPEndPoint IpEndPoint { get; }
}
