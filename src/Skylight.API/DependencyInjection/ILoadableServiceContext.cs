namespace Skylight.API.DependencyInjection;

public interface ILoadableServiceContext
{
	public Task<T> RequestDependencyAsync<T>(CancellationToken cancellationToken = default);

	public void Commit(Action action);
}
