using Skylight.API.Game.Catalog;
using Skylight.API.Game.Inventory.Items;
using Skylight.API.Game.Purse;

namespace Skylight.Server.Game.Catalog;

internal sealed class CatalogTransactionResult(ICatalogTransactionResult.ResultType result, IEnumerable<KeyValuePair<ICurrency, int>> balanceUpdate, IEnumerable<IInventoryItem> createdItems) : ICatalogTransactionResult
{
	internal static CatalogTransactionResult Success { get; } = new(ICatalogTransactionResult.ResultType.Success, [], []);
	internal static CatalogTransactionResult InsufficientFunds { get; } = new(ICatalogTransactionResult.ResultType.InsufficientFunds, [], []);

	public ICatalogTransactionResult.ResultType Result { get; } = result;
	public IEnumerable<KeyValuePair<ICurrency, int>> BalanceUpdate { get; } = balanceUpdate;
	public IEnumerable<IInventoryItem> CreatedItems { get; } = createdItems;
}
