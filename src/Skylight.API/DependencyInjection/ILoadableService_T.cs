namespace Skylight.API.DependencyInjection;

public interface ILoadableService<T> : ILoadableService
{
	public new Task<T> LoadAsync(ILoadableServiceContext context, CancellationToken cancellationToken = default);

	public T Current { get; }

	Task ILoadableService.LoadAsync(ILoadableServiceContext context, CancellationToken cancellationToken) => this.LoadAsync(context, cancellationToken);
}
