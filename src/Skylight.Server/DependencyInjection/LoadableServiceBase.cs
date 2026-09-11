using Skylight.API.DependencyInjection;

namespace Skylight.Server.DependencyInjection;

internal abstract class LoadableServiceBase<T> : ILoadableService<T>
	where T : class, IServiceSnapshot
{
	private readonly TaskCompletionSource<T> initialLoadTaskCompletionSource;

	private ValueTask<T> currentValueTask;

	private protected LoadableServiceBase()
	{
		this.initialLoadTaskCompletionSource = new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);

		this.currentValueTask = new ValueTask<T>(this.initialLoadTaskCompletionSource.Task);
	}

	public abstract Task<T> LoadAsyncCore(ILoadableServiceContext context, CancellationToken cancellationToken = default);

	public async Task<T> LoadAsync(ILoadableServiceContext context, CancellationToken cancellationToken = default)
	{
		T current = await this.LoadAsyncCore(context, cancellationToken).ConfigureAwait(false);

		context.Commit(() =>
		{
			//Ensure race conditions do not store different references
			lock (this)
			{
				this.currentValueTask = new ValueTask<T>(current);

				this.initialLoadTaskCompletionSource.TrySetResult(current);
			}
		});

		return current;
	}

	public ValueTask<T> GetAsync() => this.currentValueTask;
}
