using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Skylight.Settings.Game.Catalog.Recycler;
using Skylight.Settings.Game.Navigator;
using Skylight.Settings.Game.Rooms;
using Skylight.Settings.Net;

namespace Skylight.Settings;

public static class ServiceCollectionSettingsExtensions
{
	public static IServiceCollection ConfigureSkylightSettings(this IServiceCollection services, IConfiguration configuration)
	{
		services.Configure<FurniMaticSettings>(configuration.GetSection("FurniMatic"));
		services.Configure<NetworkSettings>(configuration.GetSection("Network"));
		services.Configure<RoomSettings>(configuration.GetSection("Room"));
		services.Configure<NavigatorSettings>(configuration.GetSection("Navigator"));

		return services;
	}
}
