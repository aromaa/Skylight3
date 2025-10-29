using Microsoft.EntityFrameworkCore;
using Skylight.API.DependencyInjection;
using Skylight.API.Game.Badges;
using Skylight.API.Game.Catalog;
using Skylight.API.Game.Furniture;
using Skylight.API.Game.Permissions;
using Skylight.API.Registry;
using Skylight.Domain.Catalog;
using Skylight.Infrastructure;
using Skylight.Server.DependencyInjection;

namespace Skylight.Server.Game.Catalog;

internal sealed partial class CatalogManager(IDbContextFactory<SkylightContext> dbContextFactory, IRegistryHolder registryHolder, IFurnitureManager furnitureManager, ICatalogTransactionFactory catalogTransactionFactory)
	: LoadableServiceBase<ICatalogSnapshot>(new Snapshot(catalogTransactionFactory, Cache.CreateBuilder().ToImmutable(registryHolder, furnitureManager.Current))), ICatalogManager
{
	private readonly IRegistryHolder registryHolder = registryHolder;

	private readonly IDbContextFactory<SkylightContext> dbContextFactory = dbContextFactory;

	private readonly ICatalogTransactionFactory catalogTransactionFactory = catalogTransactionFactory;

	public override async Task<ICatalogSnapshot> LoadAsyncCore(ILoadableServiceContext context, CancellationToken cancellationToken = default)
	{
		Task<IPermissionManager> permissionManager = context.RequestServiceAsync<IPermissionManager>(cancellationToken);
		Task<IBadgeSnapshot> badgeSnapshot = context.RequestDependencyAsync<IBadgeSnapshot>(cancellationToken);
		Task<IFurnitureSnapshot> furnitureSnapshot = context.RequestDependencyAsync<IFurnitureSnapshot>(cancellationToken);

		Cache.Builder builder = Cache.CreateBuilder();

		await using (SkylightContext dbContext = await this.dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
		{
			await foreach (RetailCatalogEntity catalog in dbContext.RetailCatalogs
				.AsNoTrackingWithIdentityResolution()
				.AsSplitQuery()
				.Include(p => p.AccessSet!)
					.ThenInclude(p => p.Rules)
				.Include(p => p.Views!)
					.ThenInclude(p => p.Parent!)
				.Include(p => p.Views!)
					.ThenInclude(p => p.Page!)
					.ThenInclude(p => p.Localization!)
					.ThenInclude(p => p.Entries)
				.Include(p => p.Views!)
					.ThenInclude(p => p.AccessSet!)
					.ThenInclude(p => p.Rules!)
				.Include(p => p.Views!.OrderBy(p => p.ParentId).ThenBy(p => p.OrderNum).ThenBy(p => p.Page!.Localization!.Code))
					.ThenInclude(p => p.Children!)
				.Include(p => p.Views!)
					.ThenInclude(p => p.Offers!)
					.ThenInclude(p => p.Offer)
				.Include(p => p.Views!)
					.ThenInclude(p => p.Offers!)
					.ThenInclude(p => p.Offer!.Localization!)
					.ThenInclude(p => p.Entries)
				.Include(p => p.Views!)
					.ThenInclude(p => p.Offers!)
					.ThenInclude(p => p.Offer!.Products)
				.Include(p => p.Views!)
					.ThenInclude(p => p.Offers!.OrderBy(o => o.PageOrderNum))
					.ThenInclude(p => p.Cost)
				.AsAsyncEnumerable()
				.WithCancellation(cancellationToken)
				.ConfigureAwait(false))
			{
				cancellationToken.ThrowIfCancellationRequested();

				builder.AddRetailCatalog(catalog);
			}
		}

		return new Snapshot(this.catalogTransactionFactory, await builder.ToImmutableAsync(this.registryHolder, await permissionManager.ConfigureAwait(false), await badgeSnapshot.ConfigureAwait(false), await furnitureSnapshot.ConfigureAwait(false), cancellationToken).ConfigureAwait(false));
	}
}
