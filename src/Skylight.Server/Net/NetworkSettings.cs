namespace Skylight.Server.Net;

internal sealed class NetworkSettings
{
	public bool EarlyBind { get; set; } = false;
	public bool EarlyAccept { get; set; } = false;

	public int TimeoutInSeconds { get; set; } = 30;

	public List<ListenerSettings> Listeners { get; set; } = [];

	public List<string> AdditionalProtocols { get; set; } = [];

	internal sealed class ListenerSettings
	{
		public List<string> EndPoints { get; set; } = null!;

		public string? Encoding { get; set; }

		public string Revision { get; set; } = null!;

		public string? CryptoPrime { get; set; }
		public string? CryptoGenerator { get; set; }
		public string? CryptoKey { get; set; }
		public string? CryptoPremix { get; set; }
	}
}
