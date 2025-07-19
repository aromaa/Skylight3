using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Skylight.API.Game.Catalog;
using Skylight.API.Game.Furniture;
using Skylight.API.Game.Inventory.Items;
using Skylight.API.Game.Purse;
using Skylight.API.Game.Users;
using Skylight.API.Registry;
using Skylight.Infrastructure;

namespace Skylight.Server.Game.Catalog;

internal sealed class CatalogTransactionFactory(IDbContextFactory<SkylightContext> dbContextFactory, IFurnitureInventoryItemStrategy furnitureInventoryItemStrategy)
	: ICatalogTransactionFactory
{
	private readonly IDbContextFactory<SkylightContext> dbContextFactory = dbContextFactory;

	private readonly IFurnitureInventoryItemStrategy furnitureInventoryItemStrategy = furnitureInventoryItemStrategy;

	public async Task<ICatalogTransaction> CreateTransactionAsync(IRegistry<ICurrencyType> currencyRegistry, IFurnitureSnapshot furniture, IUser user, string extraData, CancellationToken cancellationToken)
	{
		SkylightContext dbContext = await this.dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

		try
		{
			return await this.CreateTransactionAsync(currencyRegistry, furniture, dbContext, user, extraData, cancellationToken).ConfigureAwait(false);
		}
		catch
		{
			await dbContext.DisposeAsync().ConfigureAwait(false);

			throw;
		}
	}

	public async Task<ICatalogTransaction> CreateTransactionAsync(IRegistry<ICurrencyType> currencyRegistry, IFurnitureSnapshot furniture, DbConnection connection, IUser user, string extraData, CancellationToken cancellationToken)
	{
		SkylightContext dbContext = new(new DbContextOptionsBuilder<SkylightContext>().UseNpgsql(connection).Options);

		try
		{
			return await this.CreateTransactionAsync(currencyRegistry, furniture, dbContext, user, extraData, cancellationToken).ConfigureAwait(false);
		}
		catch
		{
			await dbContext.DisposeAsync().ConfigureAwait(false);

			throw;
		}
	}

	private async Task<ICatalogTransaction> CreateTransactionAsync(IRegistry<ICurrencyType> currencyRegistry, IFurnitureSnapshot furniture, SkylightContext dbContext, IUser user, string extraData, CancellationToken cancellationToken)
	{
		IDbContextTransaction transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);

		try
		{
			return new CatalogTransaction(currencyRegistry, furniture, this.furnitureInventoryItemStrategy, dbContext, transaction, user, extraData);
		}
		catch
		{
			await transaction.DisposeAsync().ConfigureAwait(false);

			throw;
		}
	}
}
