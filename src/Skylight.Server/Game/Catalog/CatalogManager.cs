using Microsoft.EntityFrameworkCore;
using Skylight.API.DependencyInjection;
using Skylight.API.Game.Badges;
using Skylight.API.Game.Catalog;
using Skylight.API.Game.Furniture;
using Skylight.Domain.Catalog;
using Skylight.Infrastructure;
using Skylight.Server.DependencyInjection;

namespace Skylight.Server.Game.Catalog;

internal sealed partial class CatalogManager(IDbContextFactory<SkylightContext> dbContextFactory, IBadgeManager badgeManager, IFurnitureManager furnitureManager, ICatalogTransactionFactory catalogTransactionFactory)
	: LoadableServiceBase<ICatalogSnapshot>(new Snapshot(catalogTransactionFactory, Cache.CreateBuilder().ToImmutable(badgeManager.Current, furnitureManager.Current))), ICatalogManager
{
	private readonly IDbContextFactory<SkylightContext> dbContextFactory = dbContextFactory;

	private readonly ICatalogTransactionFactory catalogTransactionFactory = catalogTransactionFactory;

	public override async Task<ICatalogSnapshot> LoadAsyncCore(ILoadableServiceContext context, CancellationToken cancellationToken = default)
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

		return new Snapshot(this.catalogTransactionFactory, builder.ToImmutable(await badgeSnapshot.ConfigureAwait(false), await furnitureSnapshot.ConfigureAwait(false)));
	}
}
