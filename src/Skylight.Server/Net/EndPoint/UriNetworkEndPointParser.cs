using System.Diagnostics.CodeAnalysis;
using Skylight.API.Net.EndPoint;

namespace Skylight.Server.Net.EndPoint;

internal sealed class UriNetworkEndPointParser : INetworkEndPointParser
{
	public bool TryParse(string value, [NotNullWhen(true)] out INetworkEndPoint? endPoint)
	{
		if (Uri.TryCreate(value, UriKind.Absolute, out Uri? result))
		{
			endPoint = new UriNetworkEndPoint(result);
			return true;
		}

		endPoint = null;
		return false;
	}
}
