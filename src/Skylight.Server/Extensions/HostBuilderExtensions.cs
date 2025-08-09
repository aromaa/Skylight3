using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Skylight.API.DependencyInjection;
using Skylight.API.Game.Achievements;
using Skylight.API.Game.Badges;
using Skylight.API.Game.Catalog;
using Skylight.API.Game.Clients;
using Skylight.API.Game.Figure;
using Skylight.API.Game.Furniture;
using Skylight.API.Game.Furniture.Floor;
using Skylight.API.Game.Inventory.Items;
using Skylight.API.Game.Navigator;
using Skylight.API.Game.Permissions;
using Skylight.API.Game.Purse;
using Skylight.API.Game.Recycler.FurniMatic;
using Skylight.API.Game.Rooms;
using Skylight.API.Game.Rooms.Items;
using Skylight.API.Game.Rooms.Items.Floor;
using Skylight.API.Game.Rooms.Items.Interactions;
using Skylight.API.Game.Rooms.Items.Wall;
using Skylight.API.Game.Users;
using Skylight.API.Game.Users.Authentication;
using Skylight.API.Net.Connection;
using Skylight.API.Net.EndPoint;
using Skylight.API.Net.Listener;
using Skylight.API.Registry;
using Skylight.API.Server;
using Skylight.Protocol.Packets.Outgoing.Purse;
using Skylight.Server.DependencyInjection;
using Skylight.Server.Game.Achievements;
using Skylight.Server.Game.Badges;
using Skylight.Server.Game.Catalog;
using Skylight.Server.Game.Catalog.Recycler.FurniMatic;
using Skylight.Server.Game.Clients;
using Skylight.Server.Game.Figure;
using Skylight.Server.Game.Furniture;
using Skylight.Server.Game.Furniture.Floor;
using Skylight.Server.Game.Inventory.Items;
using Skylight.Server.Game.Navigator;
using Skylight.Server.Game.Permissions;
using Skylight.Server.Game.Purse;
using Skylight.Server.Game.Rooms;
using Skylight.Server.Game.Rooms.Items.Domains;
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
using Skylight.Server.Registry;
using Skylight.Server.Scheduling;

namespace Skylight.Server.Extensions;

public static class HostBuilderExtensions
{
	//TODO: Actual application builder, now for simplicity
	public static IServiceCollection ConfigureSkylightServer(this IServiceCollection builder, IConfiguration configuration)
	{
		builder.AddHostedService<ServerHostService>();
		builder.AddHostedService<BackgroundWorkerService>();

		builder.Configure<FurniMaticSettings>(configuration.GetSection("FurniMatic"));
		builder.Configure<NetworkSettings>(configuration.GetSection("Network"));
		builder.Configure<RoomSettings>(configuration.GetSection("Room"));

		builder.AddSingleton(_ => TimeProvider.System);

		builder.AddSingleton<IServer, SkylightServer>();

		builder.AddSingleton<INetworkEndPointStrategy, NetworkEndPointStrategy>();
		builder.AddSingleton<INetworkEndPointParser, IpNetworkEndPointParser>();
		builder.AddSingleton<INetworkEndPointParser, UriNetworkEndPointParser>();

		builder.AddSingleton<INetworkListenerStrategy, NetworkListenerStrategy>();
		builder.AddSingleton<INetworkListenerFactory, XmlSocketNetworkListenerFactory>();
		builder.AddSingleton<INetworkListenerFactory, TcpNetworkListenerFactory>();

		builder.AddSingleton<INetworkConnectionHandler, NetworkConnectionHandler>();

		builder.AddSingleton<ILoadableServiceManager, LoadableServiceManager>();

		builder.AddSingleton<IUserManager, UserManager>();
		builder.AddSingleton<IUserAuthentication, UserAuthentication>();

		builder.AddLoadableSingleton<IPermissionManager, PermissionManager>();

		builder.AddSingleton<IClientManager, ClientManager>();
		builder.AddSingleton<PacketManagerCache>();
		builder.AddSingleton<NetworkManager>();

		builder.AddLoadableSingleton<IBadgeManager, BadgeManager>();
		builder.AddLoadableSingleton<IAchievementManager, AchievementManager>();
		builder.AddLoadableSingleton<IFigureConfigurationManager, FigureConfigurationManager>();

		builder.AddLoadableSingleton<IFurnitureManager, FurnitureManager>();
		builder.AddLoadableSingleton<ICatalogManager, CatalogManager>();
		builder.AddSingleton<ICatalogTransactionFactory, CatalogTransactionFactory>();
		builder.AddLoadableSingleton<IFurniMaticManager, FurniMaticManager>();

		builder.AddSingleton<IFurnitureInventoryItemStrategy, FurnitureInventoryItemStrategy>();

		builder.AddLoadableSingleton<IRoomManager, RoomManager>();
		builder.AddSingleton<IRoomItemInteractionManager, RoomItemInteractionManager>();
		builder.AddLoadableSingleton<INavigatorManager, NavigatorManager>();

		builder.AddSingleton<IFloorRoomItemStrategy, FloorRoomItemStrategy>();
		builder.AddSingleton<IWallRoomItemStrategy, WallRoomItemStrategy>();
		builder.AddSingleton(typeof(IFloorRoomItemStrategy<,>), typeof(FloorRoomItemStrategy<,>));
		builder.AddSingleton(typeof(IWallRoomItemStrategy<,>), typeof(WallRoomItemStrategy<,>));

		builder.AddSingleton(typeof(Lazy<>), typeof(LazyService<>));

		builder.AddBackgroundWorker<DatabaseBackgroundWorker>();

		//TODO: Figure out something nicer than this
		builder.AddSingleton<IRegistry>(Registry<IFloorFurnitureKindType>.Create(RegistryTypes.FloorFurnitureKind,
			(FloorFurnitureKindTypes.Seat.Key, new FloorFurnitureKindType(new FloorFurnitureKind())),
			(FloorFurnitureKindTypes.Walkable.Key, new FloorFurnitureKindType(new FloorFurnitureKind())),
			(FloorFurnitureKindTypes.Bed.Key, new FloorFurnitureKindType(new FloorFurnitureKind())),
			(FloorFurnitureKindTypes.Obstacle.Key, new FloorFurnitureKindType(new FloorFurnitureKind()))));

		builder.AddSingleton<IRegistry>(Registry<ICurrencyType>.Create(RegistryTypes.Currency,
			(CurrencyTypes.ActivityPoints.Key, new ActivityPointsCurrencyType()),
			(CurrencyTypes.Credits.Key, new SimpleCurrencyType((c, u) => c.SendAsync(new CreditBalanceOutgoingPacket(u)))),
			(CurrencyTypes.Silver.Key, new SimpleCurrencyType((_, _) => { }))));

		builder.AddSingleton<IRegistry>(Registry<IRoomItemDomain>.Create(RegistryTypes.RoomItemDomain,
			(RoomItemDomains.Normal.Key, new NormalRoomItemDomain()),
			(RoomItemDomains.BuildersClub.Key, new BuildersClubRoomItemDomain()),
			(RoomItemDomains.Transient.Key, new TransientRoomItemDomain())));

		builder.AddSingleton<IRegistryHolder>(l => l.GetRequiredService<IServer>());

		return builder;
	}

	public static IServiceCollection AddLoadableSingleton<TService, TImplementation>(this IServiceCollection services)
		where TService : class
		where TImplementation : class, TService, ILoadableService

	{
		services.AddSingleton<TService, TImplementation>();
		services.AddSingleton<ILoadableService, TImplementation>(l => (TImplementation)l.GetRequiredService<TService>());

		return services;
	}

	internal static IServiceCollection AddBackgroundWorker<T>(this IServiceCollection services)
		where T : BackgroundWorker
	{
		services.AddSingleton<T>();
		services.AddSingleton<BackgroundWorker, T>(l => l.GetRequiredService<T>());

		return services;
	}
}
