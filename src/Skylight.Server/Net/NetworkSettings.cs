namespace Skylight.Server.Net;

internal sealed class NetworkSettings
{
	public List<ListenerSettings> Listeners { get; set; } = [];

	public List<string> AdditionalProtocols { get; set; } = [];

	public bool EarlyBind { get; set; } = false;
	public bool EarlyAccept { get; set; } = false;

	internal sealed class ListenerSettings
	{
		public List<string> EndPoints { get; set; } = null!;

		public string Revision { get; set; } = null!;

		public string? CryptoPrime { get; set; }
		public string? CryptoGenerator { get; set; }
	}
}
