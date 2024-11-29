using System.Diagnostics;
using System.Reflection;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Skylight.API.Net.Listener;
using Skylight.Bootstrap.Attributes;
using Skylight.Bootstrap.DependencyInjection;
using Skylight.Infrastructure;
using Skylight.Plugin.WebSockets;
using Skylight.Server.Extensions;
using Skylight.Server.Host;

long now = Stopwatch.GetTimestamp();

Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
builder.ConfigureContainer(new LayeredServiceProviderFactory(), layeredBuilder =>
{
	layeredBuilder.Configure(ServiceLayer.Bootstrap, bootstrapLayer =>
	{
		IConfigurationSection database = builder.Configuration.GetSection("Database");

		bootstrapLayer.AddPooledDbContextFactory<SkylightContext>(options => BaseSkylightContext.ConfigureNpgsqlDbContextOptions(options, database["ConnectionString"])
			.EnableThreadSafetyChecks(false));
	}, provider =>
	{
		builder.Configuration.Sources.Insert(0, new ServerConfigurationSource(provider.GetRequiredService<IDbContextFactory<SkylightContext>>()));

		AddDebugProtocols(builder);
	});

	layeredBuilder.Configure(ServiceLayer.Platform, platformLayer =>
	{
		platformLayer.ConfigureSkylightServer(builder.Configuration);

		//TODO: Add proper plugin system
		platformLayer.AddSingleton<INetworkListenerFactory, WebSocketNetworkListenerFactory>();
	});
});

IHost host = builder.Build();

ILogger logger = (ILogger)host.Services.GetRequiredService(typeof(ILogger<>).MakeGenericType(typeof(Program)));

try
{
	try
	{
		await host.StartAsync().ConfigureAwait(false);
	}
	catch (TaskCanceledException)
	{
		return;
	}

	logger.LogInformation("Server started in {elapsedTime:##.###}s!", Stopwatch.GetElapsedTime(now).TotalSeconds);

	await host.WaitForShutdownAsync().ConfigureAwait(false);
}
finally
{
	if (host is IAsyncDisposable asyncDisposable)
	{
		await asyncDisposable.DisposeAsync().ConfigureAwait(false);
	}
	else
	{
		host.Dispose();
	}
}

[Conditional("DEBUG")]
static void AddDebugProtocols(HostApplicationBuilder builder)
{
	int i = 0;
	Dictionary<string, string?> values = [];
	foreach (InternalProtocolLibraryPathAttribute libraryPath in typeof(Program).Assembly.GetCustomAttributes<InternalProtocolLibraryPathAttribute>())
	{
		values[$"Network:AdditionalProtocols:{i++}"] = libraryPath.Path;
	}

	builder.Configuration.AddInMemoryCollection(values);
}
