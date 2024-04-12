using System.Diagnostics.CodeAnalysis;
using Skylight.API.Net.EndPoint;

namespace Skylight.Server.Net.EndPoint;

internal sealed class NetworkEndPointStrategy(IEnumerable<INetworkEndPointParser> parsers) : INetworkEndPointStrategy
{
	private readonly List<INetworkEndPointParser> parsers = [.. parsers];

	public bool TryParse(string value, [NotNullWhen(true)] out INetworkEndPoint? endPoint)
	{
		foreach (INetworkEndPointParser parser in this.parsers)
		{
			if (parser.TryParse(value, out endPoint))
			{
				return true;
			}
		}

		endPoint = null;
		return false;
	}
}
