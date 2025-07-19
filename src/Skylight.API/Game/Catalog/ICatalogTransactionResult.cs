using Skylight.API.Game.Inventory.Items;
using Skylight.API.Game.Purse;

namespace Skylight.API.Game.Catalog;

public interface ICatalogTransactionResult
{
	public ResultType Result { get; }

	public IEnumerable<KeyValuePair<ICurrency, int>> BalanceUpdate { get; }
	public IEnumerable<IInventoryItem> CreatedItems { get; }

	public enum ResultType
	{
		Success,
		Failure,
		InsufficientFunds
	}
}
