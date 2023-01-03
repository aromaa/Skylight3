using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Skylight.API.Game.Catalog;
using Skylight.API.Game.Furniture;
using Skylight.API.Game.Furniture.Floor;
using Skylight.API.Game.Inventory.Items;
using Skylight.API.Game.Recycler.FurniMatic;
using Skylight.Domain.Recycler.FurniMatic;
using Skylight.Infrastructure;

namespace Skylight.Server.Game.Catalog.Recycler.FurniMatic;

internal sealed partial class FurniMaticManager : IFurniMaticManager
{
	private readonly IDbContextFactory<SkylightContext> dbContextFactory;

	private readonly IFurnitureManager furnitureManager;
	private readonly IFurnitureInventoryItemStrategy furnitureInventoryItemStrategy;
	private readonly ICatalogTransactionFactory catalogTransactionFactory;

	private readonly FurniMaticSettings settings;

	private Snapshot snapshot;

	public FurniMaticManager(IDbContextFactory<SkylightContext> dbContextFactory, IFurnitureManager furnitureManager, ICatalogTransactionFactory catalogTransactionFactory, IFurnitureInventoryItemStrategy furnitureInventoryItemStrategy, IOptions<FurniMaticSettings> settings)
	{
		this.dbContextFactory = dbContextFactory;

		this.furnitureManager = furnitureManager;
		this.furnitureInventoryItemStrategy = furnitureInventoryItemStrategy;
		this.catalogTransactionFactory = catalogTransactionFactory;

		this.settings = settings.Value;

		this.snapshot = new Snapshot(this, Cache.CreateBuilder().ToImmutable(furnitureManager.Current));
	}

	public IFurniMaticSnapshot Current => this.snapshot;

	public async Task<IFurniMaticSnapshot> LoadAsync(CancellationToken cancellationToken)
	{
		IFurnitureSnapshot furnitures = this.furnitureManager.Current;

		Cache.Builder builder = Cache.CreateBuilder();

		await using (SkylightContext dbContext = await this.dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
		{
			await foreach (FurniMaticPrizeLevelEntity prizeLevel in dbContext.FurniMaticPrizeLevels
						 .AsNoTracking()
						 .AsSplitQuery()
						 .Include(l => l.Prizes!)
						 .ThenInclude(p => p.Items)
						 .AsAsyncEnumerable()
						 .WithCancellation(cancellationToken))
			{
				builder.AddLevel(prizeLevel);
			}

			if (furnitures.TryGetFloorFurniture(this.settings.GiftFurnitureId, out IFloorFurniture? giftFurniture) && giftFurniture is IFurniMaticGiftFurniture)
			{
				builder.GiftFurniture = giftFurniture;
			}
			else if (this.settings.GiftFurnitureId > 0)
			{
				throw new InvalidOperationException($"Gift furniture with id {this.settings.GiftFurnitureId} not found");
			}
		}

		return this.snapshot = new Snapshot(this, builder.ToImmutable(furnitures));
	}
}
