using System.Data.Common;
using System.Diagnostics;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Storage;
using Skylight.API.Game.Catalog;
using Skylight.API.Game.Furniture;
using Skylight.API.Game.Furniture.Floor;
using Skylight.API.Game.Furniture.Wall;
using Skylight.API.Game.Inventory.Items;
using Skylight.API.Game.Users;
using Skylight.Domain.Items;
using Skylight.Infrastructure;

namespace Skylight.Server.Game.Catalog;

internal sealed class CatalogTransaction : ICatalogTransaction
{
	private readonly IFurnitureSnapshot furnitures;
	private readonly IFurnitureInventoryItemStrategy furnitureInventoryItemStrategy;

	private readonly SkylightContext dbContext;
	private readonly IDbContextTransaction transaction;

	private readonly IUser user;

	public string ExtraData { get; }

	private List<FloorItemEntity>? floorItems;
	private List<WallItemEntity>? wallItems;

	internal CatalogTransaction(IFurnitureSnapshot furnitures, IFurnitureInventoryItemStrategy furnitureInventoryItemStrategy, SkylightContext dbContext, IDbContextTransaction transaction, IUser user, string extraData)
	{
		this.furnitures = furnitures;
		this.furnitureInventoryItemStrategy = furnitureInventoryItemStrategy;

		this.dbContext = dbContext;
		this.transaction = transaction;

		this.user = user;

		this.ExtraData = extraData;
	}

	public DbTransaction Transaction => this.transaction.GetDbTransaction();

	public void AddFloorItem(IFloorFurniture furniture, JsonDocument? extraData)
	{
		Debug.Assert(this.furnitures.TryGetFloorFurniture(furniture.Id, out IFloorFurniture? debugFurniture) && debugFurniture == furniture);

		FloorItemEntity entity = new()
		{
			FurnitureId = furniture.Id,
			UserId = this.user.Profile.Id,

			ExtraData = extraData
		};

		this.floorItems ??= new List<FloorItemEntity>();
		this.floorItems.Add(entity);
		this.dbContext.Add(entity);
	}

	public void AddWallItem(IWallFurniture furniture, JsonDocument? extraData)
	{
		Debug.Assert(this.furnitures.TryGetWallFurniture(furniture.Id, out IWallFurniture? debugFurniture) && debugFurniture == furniture);

		WallItemEntity entity = new()
		{
			FurnitureId = furniture.Id,
			UserId = this.user.Profile.Id,

			ExtraData = extraData
		};

		this.wallItems ??= new List<WallItemEntity>();
		this.wallItems.Add(entity);
		this.dbContext.Add(entity);
	}

	public async Task CompleteAsync(CancellationToken cancellationToken)
	{
		await this.dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
		await this.transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
	}

	public void Dispose() => this.DisposeAsync().GetAwaiter().GetResult();

	public async ValueTask DisposeAsync()
	{
		await this.dbContext.DisposeAsync().ConfigureAwait(false);
		await this.transaction.DisposeAsync().ConfigureAwait(false);

		List<IInventoryItem> items = new();
		if (this.floorItems is not null)
		{
			foreach (FloorItemEntity item in this.floorItems)
			{
				this.furnitures.TryGetFloorFurniture(item.FurnitureId, out IFloorFurniture? furniture);

				items.Add(this.furnitureInventoryItemStrategy.CreateFurnitureItem(item.Id, this.user.Profile, furniture!, item.ExtraData));
			}
		}

		if (this.wallItems is not null)
		{
			foreach (WallItemEntity item in this.wallItems)
			{
				this.furnitures.TryGetWallFurniture(item.FurnitureId, out IWallFurniture? furniture);

				items.Add(this.furnitureInventoryItemStrategy.CreateFurnitureItem(item.Id, this.user.Profile, furniture!, item.ExtraData));
			}
		}

		this.user.Inventory.AddUnseenItems(items);
	}
}
