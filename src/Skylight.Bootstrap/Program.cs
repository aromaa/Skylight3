using Microsoft.Extensions.Hosting;
using Skylight.Server.Extensions;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
builder.ConfigureSkylightServer();

await builder.Build().RunAsync().ConfigureAwait(false);
