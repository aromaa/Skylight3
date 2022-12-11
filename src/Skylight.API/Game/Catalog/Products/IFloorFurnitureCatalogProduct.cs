using Skylight.API.Game.Furniture;
using Skylight.API.Game.Furniture.Floor;

namespace Skylight.API.Game.Catalog.Products;

public interface IFloorFurnitureCatalogProduct : IFurnitureCatalogProduct
{
	public new IFloorFurniture Furniture { get; }

	IFurniture IFurnitureCatalogProduct.Furniture => this.Furniture;
}
