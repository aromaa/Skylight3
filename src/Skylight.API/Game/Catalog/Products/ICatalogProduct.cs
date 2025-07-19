namespace Skylight.API.Game.Catalog.Products;

public interface ICatalogProduct
{
	public ValueTask ClaimAsync(ICatalogTransactionContext context, CancellationToken cancellationToken = default);
}
