namespace Skylight.API.DependencyInjection;

public interface ILoadableServiceContext
{
	public Task<T> RequestDependencyAsync<T>(CancellationToken cancellationToken = default)
		where T : IServiceSnapshot;

	public void Commit(Action action);
}
