using System.Diagnostics.CodeAnalysis;
using System.Net;
using Skylight.API.Net.EndPoint;

namespace Skylight.Server.Net.EndPoint;

internal sealed class IpNetworkEndPointParser : INetworkEndPointParser
{
	public bool TryParse(string value, [NotNullWhen(true)] out INetworkEndPoint? endPoint)
	{
		if (IPEndPoint.TryParse(value, out IPEndPoint? ipEndPoint))
		{
			endPoint = new IpNetworkEndPoint(ipEndPoint);
			return true;
		}

		endPoint = null;
		return false;
	}
}
