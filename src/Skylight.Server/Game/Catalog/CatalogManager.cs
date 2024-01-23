using Microsoft.EntityFrameworkCore;
using Skylight.API.DependencyInjection;
using Skylight.API.Game.Badges;
using Skylight.API.Game.Catalog;
using Skylight.API.Game.Furniture;
using Skylight.Domain.Catalog;
using Skylight.Infrastructure;

namespace Skylight.Server.Game.Catalog;

internal sealed partial class CatalogManager : ICatalogManager
{
	private readonly IDbContextFactory<SkylightContext> dbContextFactory;

	private readonly IBadgeManager badgeManager;

	private readonly IFurnitureManager furnitureManager;
	private readonly ICatalogTransactionFactory catalogTransactionFactory;

	private Snapshot snapshot;

	public CatalogManager(IDbContextFactory<SkylightContext> dbContextFactory, IBadgeManager badgeManager, IFurnitureManager furnitureManager, ICatalogTransactionFactory catalogTransactionFactory)
	{
		this.dbContextFactory = dbContextFactory;

		this.badgeManager = badgeManager;

		this.furnitureManager = furnitureManager;
		this.catalogTransactionFactory = catalogTransactionFactory;

		this.snapshot = new Snapshot(this, Cache.CreateBuilder().ToImmutable(badgeManager.Current, furnitureManager.Current));
	}

	public ICatalogSnapshot Current => this.snapshot;

	public async Task<ICatalogSnapshot> LoadAsync(ILoadableServiceContext context, CancellationToken cancellationToken)
	{
		Task<IBadgeSnapshot> badgeSnapshot = context.RequestDependencyAsync<IBadgeSnapshot>(cancellationToken);
		Task<IFurnitureSnapshot> furnitureSnapshot = context.RequestDependencyAsync<IFurnitureSnapshot>(cancellationToken);

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
						 .WithCancellation(cancellationToken)
						 .ConfigureAwait(false))
			{
				cancellationToken.ThrowIfCancellationRequested();

				builder.AddPage(page);
			}
		}

		Snapshot snapshot = new(this, builder.ToImmutable(await badgeSnapshot.ConfigureAwait(false), await furnitureSnapshot.ConfigureAwait(false)));

		return context.Commit(() => this.snapshot = snapshot, snapshot);
	}
}
