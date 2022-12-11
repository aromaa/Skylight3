using Skylight.API.Game.Furniture;

namespace Skylight.API.Game.Catalog.Products;

public interface IFurnitureCatalogProduct : ICatalogProduct
{
	public IFurniture Furniture { get; }

	public int Amount { get; }
}
