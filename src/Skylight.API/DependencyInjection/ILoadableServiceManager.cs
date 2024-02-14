namespace Skylight.API.DependencyInjection;

public interface ILoadableServiceManager
{
	public Task LoadAsync(CancellationToken cancellationToken = default, bool useTransaction = true);
	public Task LoadAsync(Type serviceType, CancellationToken cancellationToken = default, bool useTransaction = true);
	public Task LoadAsync(Type[] serviceTypes, CancellationToken cancellationToken = default, bool useTransaction = true);
	public Task LoadAsync<T>(CancellationToken cancellationToken = default, bool useTransaction = true);

	public Task WaitForInitialization(CancellationToken cancellationToken = default);
}
