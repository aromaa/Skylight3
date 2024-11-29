using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Skylight.Infrastructure;

namespace Skylight.Server.Host;

internal sealed class ServerConfigurationProvider : ConfigurationProvider
{
	private readonly IDbContextFactory<SkylightContext> dbContextFactory;

	internal ServerConfigurationProvider(IDbContextFactory<SkylightContext> dbContextFactory)
	{
		this.dbContextFactory = dbContextFactory;
	}

	public override void Load()
	{
		using SkylightContext dbContext = this.dbContextFactory.CreateDbContext();

		this.Data = dbContext.Settings.ToDictionary(s => s.Id, s => s.Value, StringComparer.OrdinalIgnoreCase);
	}
}
