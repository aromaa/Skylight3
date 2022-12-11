using Skylight.API.Game.Furniture;
using Skylight.API.Game.Furniture.Wall;

namespace Skylight.API.Game.Catalog.Products;

public interface IWallFurnitureCatalogProduct : IFurnitureCatalogProduct
{
	public new IWallFurniture Furniture { get; }

	IFurniture IFurnitureCatalogProduct.Furniture => this.Furniture;
}
