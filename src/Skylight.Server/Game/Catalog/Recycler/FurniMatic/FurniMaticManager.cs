using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Skylight.API.DependencyInjection;
using Skylight.API.Game.Catalog;
using Skylight.API.Game.Furniture;
using Skylight.API.Game.Furniture.Floor;
using Skylight.API.Game.Inventory.Items;
using Skylight.API.Game.Recycler.FurniMatic;
using Skylight.Domain.Recycler.FurniMatic;
using Skylight.Infrastructure;
using Skylight.Server.DependencyInjection;

namespace Skylight.Server.Game.Catalog.Recycler.FurniMatic;

internal sealed partial class FurniMaticManager : LoadableServiceBase<IFurniMaticSnapshot>, IFurniMaticManager
{
	private readonly IDbContextFactory<SkylightContext> dbContextFactory;

	private readonly IFurnitureManager furnitureManager;
	private readonly IFurnitureInventoryItemStrategy furnitureInventoryItemStrategy;
	private readonly ICatalogTransactionFactory catalogTransactionFactory;

	private readonly FurniMaticSettings settings;

	private readonly TimeProvider timeProvider;

	public FurniMaticManager(IDbContextFactory<SkylightContext> dbContextFactory, IFurnitureManager furnitureManager, ICatalogTransactionFactory catalogTransactionFactory, IFurnitureInventoryItemStrategy furnitureInventoryItemStrategy, IOptions<FurniMaticSettings> settings, TimeProvider timeProvider)
		: base(new Snapshot(dbContextFactory, furnitureManager, furnitureInventoryItemStrategy, catalogTransactionFactory, settings.Value, timeProvider, Cache.CreateBuilder().ToImmutable(furnitureManager.Current)))
	{
		this.dbContextFactory = dbContextFactory;

		this.furnitureManager = furnitureManager;
		this.furnitureInventoryItemStrategy = furnitureInventoryItemStrategy;
		this.catalogTransactionFactory = catalogTransactionFactory;

		this.settings = settings.Value;

		this.timeProvider = timeProvider;
	}

	public override async Task<IFurniMaticSnapshot> LoadAsyncCore(ILoadableServiceContext context, CancellationToken cancellationToken)
	{
		Task<IFurnitureSnapshot> furnitures = context.RequestDependencyAsync<IFurnitureSnapshot>(cancellationToken);

		Cache.Builder builder = Cache.CreateBuilder();

		await using (SkylightContext dbContext = await this.dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
		{
			await foreach (FurniMaticPrizeLevelEntity prizeLevel in dbContext.FurniMaticPrizeLevels
						 .AsNoTracking()
						 .AsSplitQuery()
						 .Include(l => l.Prizes!)
						 .ThenInclude(p => p.Items)
						 .AsAsyncEnumerable()
						 .WithCancellation(cancellationToken)
						 .ConfigureAwait(false))
			{
				cancellationToken.ThrowIfCancellationRequested();

				builder.AddLevel(prizeLevel);
			}

			if ((await furnitures.ConfigureAwait(false)).TryGetFloorFurniture(this.settings.GiftFurnitureId, out IFloorFurniture? giftFurniture) && giftFurniture is IFurniMaticGiftFurniture)
			{
				builder.GiftFurniture = giftFurniture;
			}
			else if (this.settings.GiftFurnitureId > 0)
			{
				throw new InvalidOperationException($"Gift furniture with id {this.settings.GiftFurnitureId} not found");
			}
		}

		return new Snapshot(this.dbContextFactory, this.furnitureManager, this.furnitureInventoryItemStrategy, this.catalogTransactionFactory, this.settings, this.timeProvider, builder.ToImmutable(await furnitures.ConfigureAwait(false)));
	}
}
