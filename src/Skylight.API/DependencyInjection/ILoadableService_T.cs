namespace Skylight.API.DependencyInjection;

public interface ILoadableService<T> : ILoadableService
	where T : IServiceSnapshot
{
	public T Current { get; }

	public new Task<T> LoadAsync(ILoadableServiceContext context, CancellationToken cancellationToken = default);

	public ValueTask<T> GetAsync();

	Task ILoadableService.LoadAsync(ILoadableServiceContext context, CancellationToken cancellationToken) => this.LoadAsync(context, cancellationToken);
}
