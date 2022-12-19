using Skylight.API.Game.Catalog;
using Skylight.API.Game.Catalog.Products;

namespace Skylight.Server.Game.Catalog.Products;

internal sealed class CatalogProductBadge : IBadgeCatalogProduct
{
	public string BadgeCode { get; }

	internal CatalogProductBadge(string badgeCode)
	{
		this.BadgeCode = badgeCode;
	}

	public ValueTask PurchaseAsync(ICatalogTransaction transaction, CancellationToken cancellationToken = default)
	{
		return ValueTask.CompletedTask;
	}
}
