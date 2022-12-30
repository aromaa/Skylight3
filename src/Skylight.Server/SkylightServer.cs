using Microsoft.Extensions.Hosting;
using Skylight.API.Game.Badges;
using Skylight.API.Game.Catalog;
using Skylight.API.Game.Furniture;
using Skylight.API.Game.Navigator;
using Skylight.API.Game.Recycler.FurniMatic;
using Skylight.API.Server;
using Skylight.Server.Net;
using Skylight.Server.Net.Communication;

namespace Skylight.Server;

internal sealed class SkylightServer : IServer
{
	private readonly IHostEnvironment hostEnvironment;

	private readonly IBadgeManager badgeManager;

	private readonly IFurnitureManager furnitureManager;
	private readonly ICatalogManager catalogManager;
	private readonly IFurniMaticManager furniMaticManager;

	private readonly INavigatorManager navigatorManager;

	private readonly PacketManagerCache packetManagerCache;
	private readonly NetworkManager networkManager;

	public SkylightServer(IHostEnvironment hostEnvironment, IBadgeManager badgeManager, IFurnitureManager furnitureManager, ICatalogManager catalogManager, IFurniMaticManager furniMaticManager, INavigatorManager navigatorManager, PacketManagerCache packetManagerCache, NetworkManager networkManager)
	{
		this.hostEnvironment = hostEnvironment;

		this.badgeManager = badgeManager;

		this.furnitureManager = furnitureManager;
		this.catalogManager = catalogManager;
		this.furniMaticManager = furniMaticManager;

		this.navigatorManager = navigatorManager;

		this.packetManagerCache = packetManagerCache;
		this.networkManager = networkManager;
	}

	public async Task StartAsync(CancellationToken cancellationToken)
	{
		await this.badgeManager.LoadAsync(cancellationToken).ConfigureAwait(false);

		await this.furnitureManager.LoadAsync(cancellationToken).ConfigureAwait(false);
		await this.catalogManager.LoadAsync(cancellationToken).ConfigureAwait(false);
		await this.furniMaticManager.LoadAsync(cancellationToken).ConfigureAwait(false);

		await this.navigatorManager.LoadAsync(cancellationToken).ConfigureAwait(false);

		this.packetManagerCache.ScanAppDomain();
		this.packetManagerCache.Load(this.hostEnvironment.ContentRootFileProvider.GetDirectoryContents("Protocol"));

		this.networkManager.Start();
	}

	public Task StopAsync(CancellationToken cancellationToken)
	{
		//TODO: More graceful shutdown

		return Task.CompletedTask;
	}
}
