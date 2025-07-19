using Skylight.API.Game.Catalog;
using Skylight.API.Game.Catalog.Products;
using Skylight.API.Game.Furniture.Wall;

namespace Skylight.Server.Game.Catalog.Products;

internal sealed class CatalogProductWallItem : IWallFurnitureCatalogProduct
{
	public IWallFurniture Furniture { get; }

	public int Amount { get; }

	internal CatalogProductWallItem(IWallFurniture furniture, int amount)
	{
		this.Furniture = furniture;

		this.Amount = amount;
	}

	public ValueTask ClaimAsync(ICatalogTransactionContext context, CancellationToken cancellationToken)
	{
		for (int i = 0; i < this.Amount; i++)
		{
			context.Commands.AddWallItem(this.Furniture, null);
		}

		return ValueTask.CompletedTask;
	}
}
