using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Skylight.Infrastructure;

internal sealed class SkylightContextDesignTimeContextFactory : IDesignTimeDbContextFactory<SkylightContext>
{
	public SkylightContext CreateDbContext(string[] args)
	{
		ConfigurationBuilder configurationBuilder = new();
		configurationBuilder.AddJsonFile("appsettings.json", optional: true);
		configurationBuilder.AddEnvironmentVariables();

		if (args is { Length: > 0 })
		{
			configurationBuilder.AddCommandLine(args);
		}

		IConfigurationRoot configuration = configurationBuilder.Build();

		DbContextOptionsBuilder<SkylightContext> optionsBuilder = new();
		optionsBuilder.UseNpgsql(configuration["Database:ConnectionString"]).UseSnakeCaseNamingConvention();

		return new SkylightContext(optionsBuilder.Options);
	}
}
