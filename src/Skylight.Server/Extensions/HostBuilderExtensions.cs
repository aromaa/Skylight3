using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Skylight.API.DependencyInjection;
using Skylight.API.Game.Achievements;
using Skylight.API.Game.Badges;
using Skylight.API.Game.Catalog;
using Skylight.API.Game.Clients;
using Skylight.API.Game.Furniture;
using Skylight.API.Game.Inventory.Items;
using Skylight.API.Game.Navigator;
using Skylight.API.Game.Recycler.FurniMatic;
using Skylight.API.Game.Rooms;
using Skylight.API.Game.Rooms.Items.Floor;
using Skylight.API.Game.Rooms.Items.Interactions;
using Skylight.API.Game.Rooms.Items.Wall;
using Skylight.API.Game.Users;
using Skylight.API.Game.Users.Authentication;
using Skylight.API.Net.Connection;
using Skylight.API.Net.EndPoint;
using Skylight.API.Net.Listener;
using Skylight.Infrastructure;
using Skylight.Server.DependencyInjection;
using Skylight.Server.Game.Achievements;
using Skylight.Server.Game.Badges;
using Skylight.Server.Game.Catalog;
using Skylight.Server.Game.Catalog.Recycler.FurniMatic;
using Skylight.Server.Game.Clients;
using Skylight.Server.Game.Furniture;
using Skylight.Server.Game.Inventory.Items;
using Skylight.Server.Game.Navigator;
using Skylight.Server.Game.Rooms;
using Skylight.Server.Game.Rooms.Items.Floor;
using Skylight.Server.Game.Rooms.Items.Interactions;
using Skylight.Server.Game.Rooms.Items.Wall;
using Skylight.Server.Game.Users;
using Skylight.Server.Game.Users.Authentication;
using Skylight.Server.Host;
using Skylight.Server.Net;
using Skylight.Server.Net.Communication;
using Skylight.Server.Net.EndPoint;
using Skylight.Server.Net.Listener;
using Skylight.Server.Net.Listener.Connection;
using Skylight.Server.Net.Listener.Ip;
using Skylight.Server.Net.Listener.XmlSocket;
using StackExchange.Redis;
using IServer = Skylight.API.Server.IServer;

namespace Skylight.Server.Extensions;

public static class HostBuilderExtensions
{
	//TODO: Actual application builder, now for simplicity
	public static IHostApplicationBuilder ConfigureSkylightServer(this IHostApplicationBuilder builder)
	{
		IConfigurationSection database = builder.Configuration.GetSection("Database");
		IConfigurationSection redis = builder.Configuration.GetSection("Redis");

		builder.Configuration.Add(new ServerConfigurationSource(database["ConnectionString"]));

		builder.Services.AddHostedService<ServerHostService>();

		builder.Services.Configure<FurniMaticSettings>(builder.Configuration.GetSection("FurniMatic"));
		builder.Services.Configure<NetworkSettings>(builder.Configuration.GetSection("Network"));

		builder.Services.AddSingleton(_ => TimeProvider.System);

		builder.Services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(redis["ConnectionString"] ?? "localhost"));

		builder.Services.AddPooledDbContextFactory<SkylightContext>(options => options
			////.UseModel(SkylightContextModel.Instance)
			.UseNpgsql(database["ConnectionString"])
			.UseSnakeCaseNamingConvention()
			.EnableThreadSafetyChecks(false));

		builder.Services.AddSingleton<IServer, SkylightServer>();

		builder.Services.AddSingleton<INetworkEndPointStrategy, NetworkEndPointStrategy>();
		builder.Services.AddSingleton<INetworkEndPointParser, IpNetworkEndPointParser>();
		builder.Services.AddSingleton<INetworkEndPointParser, UriNetworkEndPointParser>();

		builder.Services.AddSingleton<INetworkListenerStrategy, NetworkListenerStrategy>();
		builder.Services.AddSingleton<INetworkListenerFactory, XmlSocketNetworkListenerFactory>();
		builder.Services.AddSingleton<INetworkListenerFactory, TcpNetworkListenerFactory>();

		builder.Services.AddSingleton<INetworkConnectionHandler, NetworkConnectionHandler>();

		builder.Services.AddSingleton<ILoadableServiceManager, LoadableServiceManager>();

		builder.Services.AddSingleton<IUserManager, UserManager>();
		builder.Services.AddSingleton<IUserAuthentication, UserAuthentication>();

		builder.Services.AddSingleton<IClientManager, ClientManager>();
		builder.Services.AddSingleton<PacketManagerCache>();
		builder.Services.AddSingleton<NetworkManager>();

		builder.Services.AddLoadableSingleton<IBadgeManager, BadgeManager>();
		builder.Services.AddLoadableSingleton<IAchievementManager, AchievementManager>();

		builder.Services.AddLoadableSingleton<IFurnitureManager, FurnitureManager>();
		builder.Services.AddLoadableSingleton<ICatalogManager, CatalogManager>();
		builder.Services.AddSingleton<ICatalogTransactionFactory, CatalogTransactionFactory>();
		builder.Services.AddLoadableSingleton<IFurniMaticManager, FurniMaticManager>();

		builder.Services.AddSingleton<IFurnitureInventoryItemStrategy, FurnitureInventoryItemStrategy>();

		builder.Services.AddSingleton<IRoomManager, RoomManager>();
		builder.Services.AddSingleton<IRoomItemInteractionManager, RoomItemInteractionManager>();
		builder.Services.AddLoadableSingleton<INavigatorManager, NavigatorManager>();

		builder.Services.AddSingleton<IFloorRoomItemStrategy, FloorRoomItemStrategy>();
		builder.Services.AddSingleton<IWallRoomItemStrategy, WallRoomItemStrategy>();
		builder.Services.AddSingleton(typeof(IFloorRoomItemStrategy<,>), typeof(FloorRoomItemStrategy<,>));
		builder.Services.AddSingleton(typeof(IWallRoomItemStrategy<,>), typeof(WallRoomItemStrategy<,>));

		builder.Services.AddSingleton(typeof(Lazy<>), typeof(LazyService<>));

		return builder;
	}

	public static IServiceCollection AddLoadableSingleton<TService, TImplementation>(this IServiceCollection services)
		where TService : class, ILoadableService
		where TImplementation : class, TService

	{
		services.AddSingleton<TService, TImplementation>();
		services.AddSingleton<ILoadableService, TImplementation>(l => (TImplementation)l.GetRequiredService<TService>());

		return services;
	}
}
