using Microsoft.Extensions.DependencyInjection;

namespace Skylight.Server.DependencyInjection;

internal sealed class LazyService<T>(IServiceProvider serviceProvider) : Lazy<T>(serviceProvider.GetRequiredService<T>)
	where T : notnull;
