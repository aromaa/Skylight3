namespace Skylight.API.Game.Catalog;

public interface ICatalogTransaction : IAsyncDisposable, IDisposable
{
	public ICatalogTransactionContext Context { get; }

	public Task<ICatalogTransactionResult> CommitAsync(CancellationToken cancellationToken = default);
}
