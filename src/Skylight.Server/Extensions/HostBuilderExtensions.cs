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
using Skylight.Infrastructure;
using Skylight.Server.DependencyInjection;
using Skylight.Server.Game.Achievements;
using Skylight.Server.Game.Badges;
using Skylight.Server.Game.Catalog;
using Skylight.Server.Game.Catalog.Recycler.FurniMatic;
using Skylight.Server.Game.Clients;
using Skylight.Server.Game.Furniture;
using Skylight.Server.Game.Inventory.Items;
using Skylight.Server.Game.Inventory.Items.Floor.Factory;
using Skylight.Server.Game.Inventory.Items.Wall.Factory;
using Skylight.Server.Game.Navigator;
using Skylight.Server.Game.Rooms;
using Skylight.Server.Game.Rooms.Items.Floor.Factory;
using Skylight.Server.Game.Rooms.Items.Interactions;
using Skylight.Server.Game.Rooms.Items.Wall.Factory;
using Skylight.Server.Game.Users;
using Skylight.Server.Game.Users.Authentication;
using Skylight.Server.Host;
using Skylight.Server.Net;
using Skylight.Server.Net.Communication;
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

		builder.Services.AddDbContextFactory<SkylightContext>(options => options.UseNpgsql(database["ConnectionString"]));

		builder.Services.AddSingleton<IServer, SkylightServer>();

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
		builder.Services.AddSingleton<IFurnitureInventoryItemFactory, StickyNoteInventoryItemFactory>();
		builder.Services.AddSingleton<IFurnitureInventoryItemFactory, FurniMaticGiftInventoryItemFactory>();
		builder.Services.AddSingleton<IFurnitureInventoryItemFactory, SoundSetInventoryItemFactory>();

		builder.Services.AddSingleton<IRoomManager, RoomManager>();
		builder.Services.AddSingleton<IRoomItemInteractionManager, RoomItemInteractionManager>();
		builder.Services.AddLoadableSingleton<INavigatorManager, NavigatorManager>();

		builder.Services.AddSingleton<IFloorRoomItemStrategy, FloorRoomItemStrategy>();
		builder.Services.AddSingleton<IFloorRoomItemFactory, BasicFloorRoomItemFactory>();
		builder.Services.AddSingleton<IFloorRoomItemFactory, FurniMaticGiftRoomItemFactory>();
		builder.Services.AddSingleton<IFloorRoomItemFactory, StickyNotePoleRoomItemFactory>();
		builder.Services.AddSingleton<IFloorRoomItemFactory, SoundMachineRoomItemFactory>();
		builder.Services.AddSingleton<IFloorRoomItemFactory, RollerRoomItemFactory>();
		builder.Services.AddSingleton<IFloorRoomItemFactory, MultiStateFloorRoomItemFactory>();

		builder.Services.AddSingleton<IWallRoomItemStrategy, WallRoomItemStrategy>();
		builder.Services.AddSingleton<IWallRoomItemFactory, BasicWallRoomItemFactory>();
		builder.Services.AddSingleton<IWallRoomItemFactory, StickyNoteRoomItemFactory>();

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
