using Microsoft.Extensions.Hosting;
using Skylight.API.DependencyInjection;
using Skylight.API.Server;
using Skylight.Server.Net;
using Skylight.Server.Net.Communication;

namespace Skylight.Server;

internal sealed class SkylightServer : IServer
{
	private readonly IHostEnvironment hostEnvironment;

	private readonly PacketManagerCache packetManagerCache;
	private readonly NetworkManager networkManager;

	private readonly Lazy<ILoadableServiceManager> loadableServiceManager;

	public SkylightServer(IHostEnvironment hostEnvironment, PacketManagerCache packetManagerCache, NetworkManager networkManager, Lazy<ILoadableServiceManager> loadableServiceManager)
	{
		this.hostEnvironment = hostEnvironment;

		this.packetManagerCache = packetManagerCache;
		this.networkManager = networkManager;

		this.loadableServiceManager = loadableServiceManager;
	}

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
