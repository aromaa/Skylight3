using Microsoft.Extensions.Hosting;
using Skylight.API.DependencyInjection;
using Skylight.API.Server;
using Skylight.Server.Net;
using Skylight.Server.Net.Communication;

namespace Skylight.Server;

internal sealed class SkylightServer(IHostEnvironment hostEnvironment, PacketManagerCache packetManagerCache, NetworkManager networkManager, Lazy<ILoadableServiceManager> loadableServiceManager)
	: IServer
{
	private readonly IHostEnvironment hostEnvironment = hostEnvironment;

	private readonly PacketManagerCache packetManagerCache = packetManagerCache;
	private readonly NetworkManager networkManager = networkManager;

	private readonly Lazy<ILoadableServiceManager> loadableServiceManager = loadableServiceManager;

	public async Task StartAsync(CancellationToken cancellationToken)
	{
		this.packetManagerCache.ScanAppDomain();
		this.packetManagerCache.Load(this.hostEnvironment.ContentRootFileProvider.GetDirectoryContents("Protocol"));

		bool earlyBind = this.networkManager.Settings.EarlyBind;
		if (earlyBind)
		{
			this.networkManager.Start();
		}

		await this.loadableServiceManager.Value.LoadAsync(cancellationToken, useTransaction: !earlyBind || !this.networkManager.Settings.EarlyAccept).ConfigureAwait(false);

		if (!earlyBind)
		{
			this.networkManager.Start();
		}
	}

	public Task StopAsync(CancellationToken cancellationToken)
	{
		//TODO: More graceful shutdown

		return Task.CompletedTask;
	}
}
