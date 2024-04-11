using System.Diagnostics;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Skylight.Bootstrap.Attributes;
using Skylight.Server.Extensions;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
builder.ConfigureSkylightServer();

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
