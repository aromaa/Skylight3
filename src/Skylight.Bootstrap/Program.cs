using Microsoft.Extensions.Hosting;
using Skylight.Server.Extensions;

namespace Skylight.Bootstrap;

internal static class Program
{
	private static async Task Main(string[] args)
	{
		await Program.CreateHostBuilder(args)
			.Build()
			.RunAsync()
			.ConfigureAwait(false);
	}

	private static IHostBuilder CreateHostBuilder(string[] args) =>
		Host.CreateDefaultBuilder(args)
			.ConfigureSkylightServerDefaults();
}
