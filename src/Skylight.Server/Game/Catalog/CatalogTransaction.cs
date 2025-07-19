using Microsoft.EntityFrameworkCore.Storage;
using Skylight.API.Game.Catalog;
using Skylight.API.Game.Furniture;
using Skylight.API.Game.Inventory.Items;
using Skylight.API.Game.Purse;
using Skylight.API.Game.Users;
using Skylight.API.Registry;
using Skylight.Infrastructure;
using Skylight.Server.Extensions;

namespace Skylight.Server.Game.Catalog;

internal sealed partial class CatalogTransaction : ICatalogTransaction
{
	private readonly IRegistry<ICurrencyType> currencyRegistry;
	private readonly IFurnitureSnapshot furnitures;
	private readonly IFurnitureInventoryItemStrategy furnitureInventoryItemStrategy;

	private readonly SkylightContext dbContext;
	private readonly IDbContextTransaction dbTransaction;

	private readonly IUser user;

	private readonly TransactionContext context;

	public CatalogTransaction(IRegistry<ICurrencyType> currencyRegistry, IFurnitureSnapshot furnitures, IFurnitureInventoryItemStrategy furnitureInventoryItemStrategy, SkylightContext dbContext, IDbContextTransaction transaction, IUser user, string extraData)
	{
		this.currencyRegistry = currencyRegistry;
		this.furnitures = furnitures;
		this.furnitureInventoryItemStrategy = furnitureInventoryItemStrategy;
		this.dbContext = dbContext;
		this.dbTransaction = transaction;
		this.user = user;
		this.ExtraData = extraData;
		this.context = new TransactionContext(this);
	}

	public string ExtraData { get; }

	public ICatalogTransactionContext Context => this.context;

	public Task<ICatalogTransactionResult> CommitAsync(CancellationToken cancellationToken = default) => this.context.CompleteAsync(cancellationToken);

	public void Dispose() => this.DisposeAsync().Wait();
	public ValueTask DisposeAsync() => this.context.DisposeAsync();
}
