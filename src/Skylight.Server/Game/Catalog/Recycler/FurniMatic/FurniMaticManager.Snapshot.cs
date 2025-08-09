using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Skylight.API.Game.Catalog;
using Skylight.API.Game.Furniture;
using Skylight.API.Game.Furniture.Floor;
using Skylight.API.Game.Furniture.Wall;
using Skylight.API.Game.Inventory.Items;
using Skylight.API.Game.Inventory.Items.Floor;
using Skylight.API.Game.Inventory.Items.Wall;
using Skylight.API.Game.Purse;
using Skylight.API.Game.Recycler.FurniMatic;
using Skylight.API.Game.Rooms.Items;
using Skylight.API.Game.Rooms.Items.Floor;
using Skylight.API.Game.Users;
using Skylight.API.Registry;
using Skylight.Domain.Items;
using Skylight.Domain.Recycler.FurniMatic;
using Skylight.Infrastructure;

namespace Skylight.Server.Game.Catalog.Recycler.FurniMatic;

internal partial class FurniMaticManager
{
	public IFurniMaticPrizes Prizes => this.Current.Prizes;

	public int ItemsRequiredToRecycle => this.Current.ItemsRequiredToRecycle;

	public Task<IFurniMaticPrize?> RecycleAsync(IUser user, IEnumerable<IFurnitureInventoryItem> items, CancellationToken cancellationToken) => this.Current.RecycleAsync(user, items, cancellationToken);
	public Task<IFurniMaticPrize?> OpenGiftAsync(IUser user, IFurniMaticGiftRoomItem gift, CancellationToken cancellationToken) => this.Current.OpenGiftAsync(user, gift, cancellationToken);

	private sealed class Snapshot : IFurniMaticSnapshot
	{
		private readonly IRegistry<ICurrencyType> currencyRegistry;
		private readonly IRoomItemDomain normalRoomItemDomain;
		private readonly IDbContextFactory<SkylightContext> dbContextFactory;

		private readonly IFurnitureManager furnitureManager;
		private readonly IFurnitureInventoryItemStrategy furnitureInventoryItemStrategy;
		private readonly ICatalogTransactionFactory catalogTransactionFactory;

		private readonly TimeProvider timeProvider;

		private readonly FurniMaticSettings settings;

		private readonly Cache cache;

		internal Snapshot(IRegistry<ICurrencyType> currencyRegistry, IRoomItemDomain normalRoomItemDomain, IDbContextFactory<SkylightContext> dbContextFactory, IFurnitureManager furnitureManager, IFurnitureInventoryItemStrategy furnitureInventoryItemStrategy, ICatalogTransactionFactory catalogTransactionFactory, FurniMaticSettings settings, TimeProvider timeProvider, Cache cache)
		{
			this.currencyRegistry = currencyRegistry;
			this.normalRoomItemDomain = normalRoomItemDomain;
			this.dbContextFactory = dbContextFactory;

			this.furnitureManager = furnitureManager;
			this.furnitureInventoryItemStrategy = furnitureInventoryItemStrategy;
			this.catalogTransactionFactory = catalogTransactionFactory;

			this.settings = settings;

			this.timeProvider = timeProvider;

			this.cache = cache;
		}

		public IFurniMaticPrizes Prizes => this.cache.Prizes;

		public int ItemsRequiredToRecycle => this.settings.ItemsRequired;

		internal (IFloorFurniture? GiftFurniture, IFurniMaticPrize? Prize) RollRandomPrice() => (this.cache.GiftFurniture, this.cache.Prizes.RollRandomPrice());

		public async Task<IFurniMaticPrize?> RecycleAsync(IUser user, IEnumerable<IFurnitureInventoryItem> items, CancellationToken cancellationToken)
		{
			(IFloorFurniture? giftFurniture, IFurniMaticPrize? prize) = this.RollRandomPrice();
			if (giftFurniture is null || prize is null)
			{
				return null;
			}

			//TODO: Catalog transaction?
			FloorItemEntity giftItemEntity = new()
			{
				UserId = user.Id,
				FurnitureId = giftFurniture.Id,
				Data = new FloorItemDataEntity
				{
					ExtraData = JsonSerializer.SerializeToDocument(this.timeProvider.GetUtcNow().UtcDateTime)
				}
			};

			try
			{
				await using SkylightContext dbContext = await this.dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

				foreach (IFurnitureInventoryItem item in items)
				{
					if (!user.Inventory.TryRemoveFurniture(item))
					{
						user.Inventory.RefreshFurniture();

						return null;
					}

					if (item is IFloorInventoryItem floorItem)
					{
						dbContext.FloorItems.Remove(new FloorItemEntity
						{
							Id = floorItem.Id,
							UserId = user.Id
						});
					}
					else if (item is IWallInventoryItem wallItem)
					{
						dbContext.WallItems.Remove(new WallItemEntity
						{
							Id = wallItem.Id,
							UserId = user.Id
						});
					}
					else
					{
						throw new NotSupportedException($"Unknown furniture type {item.GetType()}");
					}
				}

				dbContext.FurniMaticGifts.Add(new FurniMaticGiftEntity
				{
					Item = giftItemEntity,
					PrizeId = prize.Id
				});

				await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
			}
			catch (DbUpdateConcurrencyException)
			{
				user.Inventory.RefreshFurniture();

				return null;
			}

			user.Inventory.AddUnseenFloorItem(this.furnitureInventoryItemStrategy.CreateFurnitureItem(giftItemEntity.Id, user.Info, giftFurniture, giftItemEntity.Data?.ExtraData));

			return prize;
		}

		public async Task<IFurniMaticPrize?> OpenGiftAsync(IUser user, IFurniMaticGiftRoomItem gift, CancellationToken cancellationToken)
		{
			if (gift.Id.Domain != this.normalRoomItemDomain)
			{
				return null;
			}

			await using SkylightContext dbContext = await this.dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

			FurniMaticGiftEntity? giftEntity = await dbContext.FurniMaticGifts.FirstOrDefaultAsync(i => i.ItemId == gift.Id.Id, cancellationToken).ConfigureAwait(false);
			if (giftEntity is null || !this.cache.Prizes.TryGetPrize(giftEntity.PrizeId, out IFurniMaticPrize? prize))
			{
				return null;
			}

			await using ICatalogTransaction transaction = await this.catalogTransactionFactory.CreateTransactionAsync(this.currencyRegistry, this.furnitureManager.Current, dbContext.Database.GetDbConnection(), user, string.Empty, cancellationToken).ConfigureAwait(false);

			transaction.Context.AddConstraint(async (_, t, cancellationToken) =>
			{
				await using (await dbContext.Database.UseTransactionAsync(t, cancellationToken).ConfigureAwait(false))
				{
					dbContext.FloorItems.Remove(new FloorItemEntity
					{
						Id = gift.Id.Id,
						UserId = gift.Owner.Id,
						RoomId = gift.Room.Info.Id
					});

					await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
				}
			});

			transaction.Context.AddCommand((t, _, _) =>
			{
				foreach (IFurniture prizeFurniture in prize.Furnitures)
				{
					//TODO: Maybe make more general "ICatalogProduct" or just use it as impl detail
					if (prizeFurniture is IStaticFloorFurniture floorFurniture)
					{
						t.Commands.AddFloorItem(floorFurniture, null);
					}
					else if (prizeFurniture is IStaticWallFurniture wallFurniture)
					{
						t.Commands.AddWallItem(wallFurniture, null);
					}
					else
					{
						throw new NotSupportedException();
					}
				}

				return ValueTask.CompletedTask;
			});

			await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);

			return prize;
		}
	}
}
