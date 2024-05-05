using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Skylight.API.Net.EndPoint;
using Skylight.API.Net.Listener;

namespace Skylight.Server.Net;

internal sealed class NetworkManager(ILogger<NetworkManager> logger, IOptions<NetworkSettings> settings, INetworkEndPointStrategy endPointStrategy, INetworkListenerStrategy networkListenerStrategy)
{
	private readonly ILogger<NetworkManager> logger = logger;

	internal NetworkSettings Settings { get; } = settings.Value;

	private readonly INetworkEndPointStrategy endPointStrategy = endPointStrategy;
	private readonly INetworkListenerStrategy networkListenerStrategy = networkListenerStrategy;

	public void Start()
	{
		foreach (NetworkSettings.ListenerSettings listenerSettings in this.Settings.Listeners)
		{
			foreach (string endPoint in listenerSettings.EndPoints)
			{
				if (!this.endPointStrategy.TryParse(endPoint, out INetworkEndPoint? endPointInstance))
				{
					this.logger.LogWarning($"Unrecognized end point {endPoint}");

					continue;
				}

				if (!this.networkListenerStrategy.TryCreateListener(endPointInstance, out INetworkListener? listener))
				{
					this.logger.LogWarning($"Unable to find appropriate listener for {endPointInstance.GetType().Name}");

					continue;
				}

				listener.Start(new NetworkListenerConfiguration
				{
					Revision = listenerSettings.Revision,
					CryptoPrime = listenerSettings.CryptoPrime,
					CryptoGenerator = listenerSettings.CryptoGenerator,
					CryptoKey = listenerSettings.CryptoKey,
					CryptoPremix = listenerSettings.CryptoPremix
				});
			}
		}
	}
}
