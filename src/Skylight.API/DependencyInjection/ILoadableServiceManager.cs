namespace Skylight.API.DependencyInjection;

public interface ILoadableServiceManager
{
	public Task LoadAsync(CancellationToken cancellationToken = default);
	public Task LoadAsync(Type serviceType, CancellationToken cancellationToken = default);
	public Task LoadAsync(Type[] serviceTypes, CancellationToken cancellationToken = default);
	public Task LoadAsync<T>(CancellationToken cancellationToken = default);

	public Task WaitForInitialization(CancellationToken cancellationToken = default);
}
