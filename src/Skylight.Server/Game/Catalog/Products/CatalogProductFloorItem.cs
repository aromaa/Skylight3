using Skylight.API.Game.Catalog;
using Skylight.API.Game.Catalog.Products;
using Skylight.API.Game.Furniture.Floor;

namespace Skylight.Server.Game.Catalog.Products;

internal sealed class CatalogProductFloorItem : IFloorFurnitureCatalogProduct
{
	public IFloorFurniture Furniture { get; }

	public int Amount { get; }

	internal CatalogProductFloorItem(IFloorFurniture furniture, int amount)
	{
		this.Furniture = furniture;

		this.Amount = amount;
	}

	public ValueTask PurchaseAsync(ICatalogTransaction transaction, CancellationToken cancellationToken)
	{
		for (int i = 0; i < this.Amount; i++)
		{
			transaction.AddFloorItem(this.Furniture, null);
		}

		return ValueTask.CompletedTask;
	}
}
