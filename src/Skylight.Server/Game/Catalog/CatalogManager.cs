using Microsoft.EntityFrameworkCore;
using Skylight.API.Game.Catalog;
using Skylight.API.Game.Furniture;
using Skylight.Domain.Catalog;
using Skylight.Infrastructure;

namespace Skylight.Server.Game.Catalog;

internal sealed partial class CatalogManager : ICatalogManager
{
	private readonly IDbContextFactory<SkylightContext> dbContextFactory;

	private readonly IFurnitureManager furnitureManager;
	private readonly ICatalogTransactionFactory catalogTransactionFactory;

	private Snapshot snapshot;

	public CatalogManager(IDbContextFactory<SkylightContext> dbContextFactory, IFurnitureManager furnitureManager, ICatalogTransactionFactory catalogTransactionFactory)
	{
		this.dbContextFactory = dbContextFactory;

		this.furnitureManager = furnitureManager;
		this.catalogTransactionFactory = catalogTransactionFactory;

		this.snapshot = new Snapshot(this, Cache.CreateBuilder().ToImmutable(furnitureManager.Current));
	}

	public ICatalogSnapshot Current => this.snapshot;

	public async Task LoadAsync(CancellationToken cancellationToken)
	{
		Cache.Builder builder = Cache.CreateBuilder();

		await using (SkylightContext dbContext = await this.dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
		{
			await foreach (CatalogPageEntity page in dbContext.CatalogPages
						 .AsNoTrackingWithIdentityResolution()
						 .AsSplitQuery()
						 .Include(p => p.Children!)
						 .Include(p => p.Offers!)
						 .ThenInclude(o => o.Products)
						 .AsAsyncEnumerable()
						 .WithCancellation(cancellationToken))
			{
				builder.AddPage(page);
			}
		}

		this.snapshot = new Snapshot(this, builder.ToImmutable(this.furnitureManager.Current));
	}
}
