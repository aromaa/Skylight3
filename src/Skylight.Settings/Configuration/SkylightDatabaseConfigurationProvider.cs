using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Skylight.Infrastructure;

namespace Skylight.Settings.Configuration;

internal sealed class SkylightDatabaseConfigurationProvider : ConfigurationProvider
{
	private readonly IDbContextFactory<SkylightContext> dbContextFactory;

	internal SkylightDatabaseConfigurationProvider(IDbContextFactory<SkylightContext> dbContextFactory)
	{
		this.dbContextFactory = dbContextFactory;
	}

	public override void Load()
	{
		using SkylightContext dbContext = this.dbContextFactory.CreateDbContext();

		this.Data = dbContext.Settings.ToDictionary(s => s.Id, s => s.Value, StringComparer.OrdinalIgnoreCase);
	}
}
