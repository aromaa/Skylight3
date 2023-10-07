using Microsoft.Extensions.DependencyInjection;

namespace Skylight.Server.DependencyInjection;

internal sealed class LazyService<T> : Lazy<T>
	where T : notnull
{
	public LazyService(IServiceProvider serviceProvider)
		: base(serviceProvider.GetRequiredService<T>)
	{
	}
}
