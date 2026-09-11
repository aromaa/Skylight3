using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Skylight.Infrastructure;

namespace Skylight.Settings.Configuration;

public sealed class SkylightDatabaseConfigurationSource(IDbContextFactory<SkylightContext> dbContextFactory) : IConfigurationSource
{
	private readonly IDbContextFactory<SkylightContext> dbContextFactory = dbContextFactory;

	public IConfigurationProvider Build(IConfigurationBuilder builder)
	{
		return new SkylightDatabaseConfigurationProvider(this.dbContextFactory);
	}
}
