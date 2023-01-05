using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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
	public static IHostBuilder ConfigureSkylightServerDefaults(this IHostBuilder builder)
	{
		return builder.ConfigureSkylightServerDefaults(static _ => { });
	}

	public static IHostBuilder ConfigureSkylightServerDefaults(this IHostBuilder builder, Action<IServerHostBuilder> configure)
	{
		return builder.ConfigureSkylightServer(hostBuilder =>
		{
			hostBuilder.ConfigureAppConfiguration(static (_, configuration) =>
			{
				IConfigurationRoot currentConfig = configuration.Build();

				configuration.Add(new ServerConfigurationSource(currentConfig["Database:ConnectionString"]));
			});

			hostBuilder.ConfigureServices(static (context, services) =>
			{
				IConfigurationSection database = context.Configuration.GetSection("Database");
				IConfigurationSection redis = context.Configuration.GetSection("Redis");

				services.Configure<FurniMaticSettings>(context.Configuration.GetSection("FurniMatic"));
				services.Configure<NetworkSettings>(context.Configuration.GetSection("Network"));

				services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(redis["ConnectionString"] ?? "localhost"));

				services.AddDbContextFactory<SkylightContext>(options => options.UseNpgsql(database["ConnectionString"]));

				services.AddSingleton<IServer, SkylightServer>();

				services.AddSingleton<IUserManager, UserManager>();
				services.AddSingleton<IUserAuthentication, UserAuthentication>();

				services.AddSingleton<IClientManager, ClientManager>();
				services.AddSingleton<PacketManagerCache>();
				services.AddSingleton<NetworkManager>();

				services.AddSingleton<IBadgeManager, BadgeManager>();
				services.AddSingleton<IAchievementManager, AchievementManager>();

				services.AddSingleton<IFurnitureManager, FurnitureManager>();
				services.AddSingleton<ICatalogManager, CatalogManager>();
				services.AddSingleton<ICatalogTransactionFactory, CatalogTransactionFactory>();
				services.AddSingleton<IFurniMaticManager, FurniMaticManager>();

				services.AddSingleton<IFurnitureInventoryItemStrategy, FurnitureInventoryItemStrategy>();
				services.AddSingleton<IFurnitureInventoryItemFactory, StickyNoteInventoryItemFactory>();
				services.AddSingleton<IFurnitureInventoryItemFactory, FurniMaticGiftInventoryItemFactory>();
				services.AddSingleton<IFurnitureInventoryItemFactory, SoundSetInventoryItemFactory>();

				services.AddSingleton<IRoomManager, RoomManager>();
				services.AddSingleton<IRoomItemInteractionManager, RoomItemInteractionManager>();
				services.AddSingleton<INavigatorManager, NavigatorManager>();

				services.AddSingleton<IFloorRoomItemStrategy, FloorRoomItemStrategy>();
				services.AddSingleton<IFloorRoomItemFactory, BasicFloorRoomItemFactory>();
				services.AddSingleton<IFloorRoomItemFactory, FurniMaticGiftRoomItemFactory>();
				services.AddSingleton<IFloorRoomItemFactory, StickyNotePoleRoomItemFactory>();
				services.AddSingleton<IFloorRoomItemFactory, SoundMachineRoomItemFactory>();

				services.AddSingleton<IWallRoomItemStrategy, WallRoomItemStrategy>();
				services.AddSingleton<IWallRoomItemFactory, BasicWallRoomItemFactory>();
				services.AddSingleton<IWallRoomItemFactory, StickyNoteRoomItemFactory>();
			});

			configure(hostBuilder);
		});
	}

	public static IHostBuilder ConfigureSkylightServer(this IHostBuilder builder, Action<IServerHostBuilder> configure)
	{
		ServerHostBuilder hostBuilder = new(builder);
		configure(hostBuilder);

		builder.ConfigureServices(static (_, services) =>
		{
			services.AddHostedService<ServerHostService>();
		});

		return builder;
	}
}
