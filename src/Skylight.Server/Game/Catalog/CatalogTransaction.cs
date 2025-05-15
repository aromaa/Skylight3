using System.Data.Common;
using System.Diagnostics;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Skylight.API.Game.Badges;
using Skylight.API.Game.Catalog;
using Skylight.API.Game.Furniture;
using Skylight.API.Game.Furniture.Floor;
using Skylight.API.Game.Furniture.Wall;
using Skylight.API.Game.Inventory.Items;
using Skylight.API.Game.Users;
using Skylight.Domain.Badges;
using Skylight.Domain.Items;
using Skylight.Domain.Users;
using Skylight.Infrastructure;
using Skylight.Server.Extensions;
using Skylight.Server.Game.Inventory.Items.Badges;

namespace Skylight.Server.Game.Catalog;

internal sealed class CatalogTransaction : ICatalogTransaction
{
	private readonly IFurnitureSnapshot furnitures;
	private readonly IFurnitureInventoryItemStrategy furnitureInventoryItemStrategy;

	private readonly SkylightContext dbContext;
	private readonly IDbContextTransaction transaction;

	public IUser User { get; }

	public string ExtraData { get; }

	private List<IBadge>? badges;

	private List<FloorItemEntity>? floorItems;
	private List<WallItemEntity>? wallItems;

	private readonly Dictionary<string, int> currencyChanges = new();

	internal CatalogTransaction(IFurnitureSnapshot furnitures, IFurnitureInventoryItemStrategy furnitureInventoryItemStrategy, SkylightContext dbContext, IDbContextTransaction transaction, IUser user, string extraData)
	{
		this.furnitures = furnitures;
		this.furnitureInventoryItemStrategy = furnitureInventoryItemStrategy;

		this.dbContext = dbContext;
		this.transaction = transaction;

		this.User = user;

		this.ExtraData = extraData;
	}

	public DbTransaction Transaction => this.transaction.GetDbTransaction();

	public void AddBadge(IBadge badge)
	{
		if (this.User.Inventory.HasBadge(badge.Code))
		{
			return;
		}

		UserBadgeEntity entity = new()
		{
			UserId = this.User.Profile.Id,
			BadgeCode = badge.Code
		};

		this.badges ??= [];
		this.badges.Add(badge);
		this.dbContext.Add(entity);
	}

	public void AddFloorItem(IFloorFurniture furniture, JsonDocument? extraData)
	{
		Debug.Assert(this.furnitures.TryGetFloorFurniture(furniture.Id, out IFloorFurniture? debugFurniture) && debugFurniture == furniture);

		FloorItemEntity entity = new()
		{
			FurnitureId = furniture.Id,
			UserId = this.User.Profile.Id
		};

		if (extraData is not null)
		{
			entity.Data = new FloorItemDataEntity
			{
				ExtraData = extraData
			};
		}

		this.floorItems ??= [];
		this.floorItems.Add(entity);
		this.dbContext.Add(entity);
	}

	public void AddWallItem(IWallFurniture furniture, JsonDocument? extraData)
	{
		Debug.Assert(this.furnitures.TryGetWallFurniture(furniture.Id, out IWallFurniture? debugFurniture) && debugFurniture == furniture);

		WallItemEntity entity = new()
		{
			FurnitureId = furniture.Id,
			UserId = this.User.Profile.Id
		};

		if (extraData is not null)
		{
			entity.Data = new WallItemDataEntity
			{
				ExtraData = extraData
			};
		}

		this.wallItems ??= [];
		this.wallItems.Add(entity);
		this.dbContext.Add(entity);
	}

	public void DeductCurrency(string currencyKey, int amount)
	{
		int currentBalance = this.User.Currencies.GetBalance(currencyKey);

		if (currentBalance < amount)
		{
			throw new InvalidOperationException("Not enough balance to complete the purchase");
		}

		int newBalance = currentBalance - amount;
		this.User.Currencies.UpdateBalance(currencyKey, newBalance);

		this.currencyChanges[currencyKey] = newBalance;
	}

	public async Task CompleteAsync(CancellationToken cancellationToken = default)
	{
		int userId = this.User.Profile.Id;

		Dictionary<string, UserCurrenciesEntity> existingCurrencies = await this.dbContext.UserCurrencies
			.Where(uc => uc.UserId == userId && this.currencyChanges.Keys.Contains(uc.Currency))
			.ToDictionaryAsync(uc => uc.Currency, uc => uc, cancellationToken)
			.ConfigureAwait(false);

		List<UserCurrenciesEntity> toAdd = [];

		foreach ((string currencyKey, int newBalance) in this.currencyChanges)
		{
			if (existingCurrencies.TryGetValue(currencyKey, out UserCurrenciesEntity? entity))
			{
				entity.Balance = newBalance;
			}
			else
			{
				toAdd.Add(new UserCurrenciesEntity { UserId = userId, Currency = currencyKey, Balance = newBalance });
			}
		}

		if (toAdd.Count > 0)
		{
			await this.dbContext.UserCurrencies
				.AddRangeAsync(toAdd, cancellationToken)
				.ConfigureAwait(false);
		}

		try
		{
			await this.dbContext.SaveChangesAsync(cancellationToken)
				.ConfigureAwait(false);

			await this.transaction.CommitAsync(cancellationToken)
				.ConfigureAwait(false);
		}
		catch
		{
			await this.transaction.RollbackAsync(cancellationToken)
				.ConfigureAwait(false);
			throw;
		}
	}

	public void Dispose() => this.DisposeAsync().Wait();

	public async ValueTask DisposeAsync()
	{
		await this.dbContext.DisposeAsync().ConfigureAwait(false);
		await this.transaction.DisposeAsync().ConfigureAwait(false);

		List<IInventoryItem> items = [];
		if (this.badges is not null)
		{
			foreach (IBadge badge in this.badges)
			{
				items.Add(new BadgeInventoryItem(badge, this.User.Profile));
			}
		}

		if (this.floorItems is not null)
		{
			foreach (FloorItemEntity item in this.floorItems)
			{
				this.furnitures.TryGetFloorFurniture(item.FurnitureId, out IFloorFurniture? furniture);

				items.Add(this.furnitureInventoryItemStrategy.CreateFurnitureItem(item.Id, this.User.Profile, furniture!, item.Data?.ExtraData));
			}
		}

		if (this.wallItems is not null)
		{
			foreach (WallItemEntity item in this.wallItems)
			{
				this.furnitures.TryGetWallFurniture(item.FurnitureId, out IWallFurniture? furniture);

				items.Add(this.furnitureInventoryItemStrategy.CreateFurnitureItem(item.Id, this.User.Profile, furniture!, item.Data?.ExtraData));
			}
		}

		this.User.Inventory.AddUnseenItems(items);
	}
}
