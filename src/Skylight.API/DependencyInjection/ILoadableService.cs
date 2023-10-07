namespace Skylight.API.DependencyInjection;

public interface ILoadableService
{
	public Task LoadAsync(ILoadableServiceContext context, CancellationToken cancellationToken = default);
}
