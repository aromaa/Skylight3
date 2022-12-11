namespace Skylight.Server.Net;

internal sealed class NetworkSettings
{
	public List<ListenerSettings> Listeners { get; set; } = new();

	internal sealed class ListenerSettings
	{
		public required string EndPoint { get; set; }

		public required string Revision { get; set; }

		public string? CryptoPrime { get; set; }
		public string? CryptoGenerator { get; set; }
	}
}
