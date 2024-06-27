using System.Diagnostics;
using System.Reflection;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Skylight.API.Net.Listener;
using Skylight.Bootstrap.Attributes;
using Skylight.Plugin.WebSockets;
using Skylight.Server.Extensions;

Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
builder.ConfigureSkylightServer();

//TODO: Add proper plugin system
builder.Services.AddSingleton<INetworkListenerFactory, WebSocketNetworkListenerFactory>();

AddDebugProtocols(builder);

IHost host = builder.Build();

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
