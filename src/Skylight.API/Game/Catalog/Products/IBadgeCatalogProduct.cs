using Skylight.API.Game.Badges;

namespace Skylight.API.Game.Catalog.Products;

public interface IBadgeCatalogProduct : ICatalogProduct
{
	public IBadge Badge { get; }
}
