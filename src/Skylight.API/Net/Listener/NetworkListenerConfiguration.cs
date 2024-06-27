using System.Text;

namespace Skylight.API.Net.Listener;

public sealed class NetworkListenerConfiguration
{
	public required Encoding Encoding { get; init; }

	public string? Revision { get; init; }
	public string? CryptoPrime { get; init; }
	public string? CryptoGenerator { get; init; }
	public string? CryptoPremix { get; init; }
	public string? CryptoKey { get; init; }
}
