using Skylight.API.Game.Badges;
using Skylight.API.Game.Catalog;
using Skylight.API.Game.Catalog.Products;

namespace Skylight.Server.Game.Catalog.Products;

internal sealed class CatalogProductBadge : IBadgeCatalogProduct
{
	public IBadge Badge { get; }

	internal CatalogProductBadge(IBadge badge)
	{
		this.Badge = badge;
	}

	public ValueTask PurchaseAsync(ICatalogTransaction transaction, CancellationToken cancellationToken = default)
	{
		transaction.AddBadge(this.Badge);

		return ValueTask.CompletedTask;
	}
}
