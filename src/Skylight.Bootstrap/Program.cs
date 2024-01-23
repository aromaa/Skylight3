using Microsoft.Extensions.Hosting;
using Skylight.Server.Extensions;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
builder.ConfigureSkylightServer();

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
