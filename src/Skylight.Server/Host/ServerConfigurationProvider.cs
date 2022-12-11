using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Skylight.Infrastructure;

namespace Skylight.Server.Host;

internal sealed class ServerConfigurationProvider : ConfigurationProvider
{
	private readonly string? connectionString;

	internal ServerConfigurationProvider(string? connectionString)
	{
		this.connectionString = connectionString;
	}

	public override void Load()
	{
		using SkylightContext dbContext = new(new DbContextOptionsBuilder<SkylightContext>().UseNpgsql(this.connectionString).Options);

		this.Data = dbContext.Settings.ToDictionary(s => s.Id, s => s.Value, StringComparer.OrdinalIgnoreCase);
	}
}
