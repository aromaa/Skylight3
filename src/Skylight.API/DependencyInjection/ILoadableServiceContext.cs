namespace Skylight.API.DependencyInjection;

public interface ILoadableServiceContext
{
	public Task<T> RequestServiceAsync<T>(CancellationToken cancellationToken = default)
		where T : ILoadableService;

	public Task<T> RequestDependencyAsync<T>(CancellationToken cancellationToken = default)
		where T : IServiceSnapshot;

	public void Commit(Action action);
}
