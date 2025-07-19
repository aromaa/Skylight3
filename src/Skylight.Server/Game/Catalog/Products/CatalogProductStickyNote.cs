using System.Text.Json;
using Skylight.API.Game.Catalog;
using Skylight.API.Game.Catalog.Products;
using Skylight.API.Game.Furniture.Wall;

namespace Skylight.Server.Game.Catalog.Products;

internal sealed class CatalogProductStickyNote : IWallFurnitureCatalogProduct
{
	public IWallFurniture Furniture { get; }

	public int Amount { get; }

	internal CatalogProductStickyNote(IStickyNoteFurniture furniture, int amount)
	{
		this.Furniture = furniture;

		this.Amount = amount;
	}

	public ValueTask ClaimAsync(ICatalogTransactionContext context, CancellationToken cancellationToken)
	{
		context.Commands.AddWallItem(this.Furniture, JsonSerializer.SerializeToDocument(this.Amount));

		return ValueTask.CompletedTask;
	}
}
