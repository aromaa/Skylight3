using Skylight.API.DependencyInjection;

namespace Skylight.Server.DependencyInjection;

internal abstract class VersionedLoadableServiceBase
{
	internal abstract VersionedServiceSnapshot Current { get; }
}

internal abstract class VersionedLoadableServiceBase<TInterface, TImplementation> : VersionedLoadableServiceBase, ILoadableService<TInterface>
	where TImplementation : VersionedServiceSnapshot, TInterface
{
	private readonly TaskCompletionSource<TInterface> initialLoadTaskCompletionSource;

	private ValueTask<TInterface> currentValueTask;

	private TImplementation current;

	private int nextVersion;

	protected VersionedLoadableServiceBase(TImplementation current)
	{
		this.initialLoadTaskCompletionSource = new TaskCompletionSource<TInterface>(TaskCreationOptions.RunContinuationsAsynchronously);

		this.currentValueTask = new ValueTask<TInterface>(this.initialLoadTaskCompletionSource.Task);

		this.current = current;
	}

	internal abstract Task<VersionedServiceSnapshot.Transaction<TImplementation>> LoadAsyncCore(ILoadableServiceContext context, CancellationToken cancellationToken = default);

	public async Task<TInterface> LoadAsync(ILoadableServiceContext context, CancellationToken cancellationToken = default)
	{
		VersionedServiceSnapshot.Transaction<TImplementation> transaction = await this.LoadAsyncCore(context, cancellationToken).ConfigureAwait(false);

		context.Commit(() =>
		{
			//Ensure race conditions do not store different references
			lock (this)
			{
				TImplementation current;

				using (transaction)
				{
					current = transaction.Commit(Interlocked.Increment(ref this.nextVersion));

					this.current = current;
					this.currentValueTask = new ValueTask<TInterface>(current);
				}

				this.initialLoadTaskCompletionSource.TrySetResult(current);
			}
		});

		return transaction.Current;
	}

	internal override TImplementation Current => this.current;

	TInterface ILoadableService<TInterface>.Current => this.current;

	public ValueTask<TInterface> GetAsync() => this.currentValueTask;
}
