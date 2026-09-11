using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Skylight.API.Net.Listener;
using Skylight.Settings.Net;

namespace Skylight.Server.Net;

internal sealed class NetworkManager(ILogger<NetworkManager> logger, IOptions<NetworkSettings> settings, INetworkListenerStrategy networkListenerStrategy)
{
	private readonly ILogger<NetworkManager> logger = logger;

	internal NetworkSettings Settings { get; } = settings.Value;

	private readonly INetworkListenerStrategy networkListenerStrategy = networkListenerStrategy;

	public void Start()
	{
		foreach (NetworkSettings.ListenerSettings listenerSettings in this.Settings.Listeners.Values)
		{
			foreach (string endPoint in listenerSettings.EndPoints)
			{
				if (!Uri.TryCreate(endPoint, UriKind.Absolute, out Uri? uri))
				{
					uri = new Uri("tcp://" + endPoint, UriKind.Absolute);
				}

				if (!this.networkListenerStrategy.TryCreateListener(uri, out INetworkListener? listener))
				{
					this.logger.LogWarning($"Unable to find appropriate listener for {uri}");

					continue;
				}

				listener.Start(new NetworkListenerConfiguration
				{
					Encoding = listenerSettings.Encoding is not null ? Encoding.GetEncoding(listenerSettings.Encoding) : Encoding.UTF8,
					Revision = listenerSettings.Revision,
					CryptoPrime = listenerSettings.CryptoPrime,
					CryptoGenerator = listenerSettings.CryptoGenerator,
					CryptoKey = listenerSettings.CryptoKey,
					CryptoPremix = listenerSettings.CryptoPremix,
					DecodePremix = listenerSettings.DecodePremix
				});
			}
		}
	}
}
