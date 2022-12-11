using System.Data.Common;
using Skylight.API.Game.Furniture;
using Skylight.API.Game.Users;

namespace Skylight.API.Game.Catalog;

public interface ICatalogTransactionFactory
{
	public Task<ICatalogTransaction> CreateTransactionAsync(IFurnitureSnapshot furnitures, IUser user, string extraData, CancellationToken cancellationToken = default);
	public Task<ICatalogTransaction> CreateTransactionAsync(IFurnitureSnapshot furnitures, DbConnection connection, IUser user, string extraData, CancellationToken cancellationToken = default);
}
