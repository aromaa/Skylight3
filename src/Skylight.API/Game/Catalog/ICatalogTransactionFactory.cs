using System.Data.Common;
using Skylight.API.Game.Furniture;
using Skylight.API.Game.Purse;
using Skylight.API.Game.Users;
using Skylight.API.Registry;

namespace Skylight.API.Game.Catalog;

public interface ICatalogTransactionFactory
{
	public Task<ICatalogTransaction> CreateTransactionAsync(IRegistry<ICurrencyType> currencyRegistry, IFurnitureSnapshot furnitures, IUser user, string extraData, CancellationToken cancellationToken = default);
	public Task<ICatalogTransaction> CreateTransactionAsync(IRegistry<ICurrencyType> currencyRegistry, IFurnitureSnapshot furnitures, DbConnection connection, IUser user, string extraData, CancellationToken cancellationToken = default);
}
