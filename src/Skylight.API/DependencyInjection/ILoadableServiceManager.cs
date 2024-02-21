namespace Skylight.API.DependencyInjection;

public interface ILoadableServiceManager
{
	public Task LoadAsync(bool useTransaction = true, CancellationToken cancellationToken = default);
	public Task LoadAsync(Type serviceType, bool useTransaction = true, CancellationToken cancellationToken = default);
	public Task LoadAsync(Type[] serviceTypes, bool useTransaction = true, CancellationToken cancellationToken = default);
	public Task LoadAsync<T>(bool useTransaction = true, CancellationToken cancellationToken = default);

	public Task WaitForInitialization(CancellationToken cancellationToken = default);
}
