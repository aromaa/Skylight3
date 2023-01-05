using Microsoft.Extensions.Hosting;
using Skylight.API.Game.Achievements;
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
	private readonly IAchievementManager achievementManager;

	private readonly IFurnitureManager furnitureManager;
	private readonly ICatalogManager catalogManager;
	private readonly IFurniMaticManager furniMaticManager;

	private readonly INavigatorManager navigatorManager;

	private readonly PacketManagerCache packetManagerCache;
	private readonly NetworkManager networkManager;

	public SkylightServer(IHostEnvironment hostEnvironment, IBadgeManager badgeManager, IAchievementManager achievementManager, IFurnitureManager furnitureManager, ICatalogManager catalogManager, IFurniMaticManager furniMaticManager, INavigatorManager navigatorManager, PacketManagerCache packetManagerCache, NetworkManager networkManager)
	{
		this.hostEnvironment = hostEnvironment;

		this.badgeManager = badgeManager;
		this.achievementManager = achievementManager;

		this.furnitureManager = furnitureManager;
		this.catalogManager = catalogManager;
		this.furniMaticManager = furniMaticManager;

		this.navigatorManager = navigatorManager;

		this.packetManagerCache = packetManagerCache;
		this.networkManager = networkManager;
	}

	public async Task StartAsync(CancellationToken cancellationToken)
	{
		await new SimpleParallelLoader(cancellationToken)
			.Execute(this.badgeManager.LoadAsync)
			.Execute(this.achievementManager.LoadAsync, typeof(IBadgeSnapshot))
			.Execute(this.furnitureManager.LoadAsync)
			.Execute(this.catalogManager.LoadAsync, typeof(IBadgeSnapshot), typeof(IFurnitureSnapshot))
			.Execute(this.furniMaticManager.LoadAsync, typeof(IFurnitureSnapshot))
			.Execute(this.navigatorManager.LoadAsync)
			.WaitAsync()
			.ConfigureAwait(false);

		this.packetManagerCache.ScanAppDomain();
		this.packetManagerCache.Load(this.hostEnvironment.ContentRootFileProvider.GetDirectoryContents("Protocol"));

		this.networkManager.Start();
	}

	public Task StopAsync(CancellationToken cancellationToken)
	{
		//TODO: More graceful shutdown

		return Task.CompletedTask;
	}

	private readonly struct SimpleParallelLoader
	{
		private readonly CancellationToken cancellationToken;

		private readonly Dictionary<Type, Task> tasks;

		internal SimpleParallelLoader(CancellationToken cancellationToken)
		{
			this.cancellationToken = cancellationToken;

			this.tasks = new Dictionary<Type, Task>();
		}

		internal SimpleParallelLoader Execute<T>(Func<CancellationToken, Task<T>> action, params Type[] dependencies)
		{
			if (dependencies.Length == 0)
			{
				this.tasks.Add(typeof(T), action(this.cancellationToken));
			}
			else
			{
				Task[] dependencyTasks = new Task[dependencies.Length];
				for (int i = 0; i < dependencies.Length; i++)
				{
					dependencyTasks[i] = this.tasks[dependencies[i]];
				}

				CancellationToken cancellationToken = this.cancellationToken;

				this.tasks.Add(typeof(T), Task.WhenAll(dependencyTasks).ContinueWith(_ => action(cancellationToken), cancellationToken).Unwrap());
			}

			return this;
		}

		internal Task WaitAsync() => Task.WhenAll(this.tasks.Values);
	}
}
