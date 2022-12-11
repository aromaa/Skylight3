namespace Skylight.API.Game.Catalog.Products;

public interface ICatalogProduct
{
	public ValueTask PurchaseAsync(ICatalogTransaction transaction, CancellationToken cancellationToken = default);
}
