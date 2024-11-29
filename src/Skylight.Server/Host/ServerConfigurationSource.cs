using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Skylight.Infrastructure;

namespace Skylight.Server.Host;

public sealed class ServerConfigurationSource(IDbContextFactory<SkylightContext> dbContextFactory) : IConfigurationSource
{
	private readonly IDbContextFactory<SkylightContext> dbContextFactory = dbContextFactory;

	public IConfigurationProvider Build(IConfigurationBuilder builder)
	{
		return new ServerConfigurationProvider(this.dbContextFactory);
	}
}
