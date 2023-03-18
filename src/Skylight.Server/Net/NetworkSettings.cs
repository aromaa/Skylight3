namespace Skylight.Server.Net;

internal sealed class NetworkSettings
{
	public List<ListenerSettings> Listeners { get; set; } = new();

	internal sealed class ListenerSettings
	{
		public required List<string> EndPoints { get; set; }

		public required string Revision { get; set; }

		public string? CryptoPrime { get; set; }
		public string? CryptoGenerator { get; set; }
	}
}
