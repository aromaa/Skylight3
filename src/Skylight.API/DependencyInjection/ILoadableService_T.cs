namespace Skylight.API.DependencyInjection;

public interface ILoadableService<T> : ILoadableService
	where T : IServiceSnapshot
{
	public ValueTask<T> GetAsync();

	public new Task<T> LoadAsync(ILoadableServiceContext context, CancellationToken cancellationToken = default);

	Task ILoadableService.LoadAsync(ILoadableServiceContext context, CancellationToken cancellationToken) => this.LoadAsync(context, cancellationToken);
}
