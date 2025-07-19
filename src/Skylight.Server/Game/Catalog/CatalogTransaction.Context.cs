using System.Data.Common;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.Json;
using LinqToDB;
using LinqToDB.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Skylight.API.Game.Badges;
using Skylight.API.Game.Catalog;
using Skylight.API.Game.Furniture.Floor;
using Skylight.API.Game.Furniture.Wall;
using Skylight.API.Game.Inventory.Items;
using Skylight.API.Game.Purse;
using Skylight.API.Game.Users;
using Skylight.Domain.Badges;
using Skylight.Domain.Items;
using Skylight.Domain.Users;
using Skylight.Server.Game.Inventory.Items.Badges;

namespace Skylight.Server.Game.Catalog;

internal partial class CatalogTransaction
{
	private sealed class TransactionContext(CatalogTransaction transaction) : ICatalogTransactionContext, ICatalogTransactionContext.IConstraints, ICatalogTransactionContext.ICommands
	{
		private readonly CatalogTransaction transaction = transaction;

		private List<Func<ICatalogTransactionContext, DbTransaction, CancellationToken, ValueTask>>? constraints;
		private List<Func<ICatalogTransactionContext, DbTransaction, CancellationToken, ValueTask>>? commands;

		private Dictionary<(string Key, string? Data), (ICurrency Currency, int Change)>? balanceChanges;

		private bool dbContextDirty;

		private List<IBadge>? badges;

		private List<FloorItemEntity>? floorItems;
		private List<WallItemEntity>? wallItems;

		private Dictionary<ICurrency, int>? purse;

		private bool ConstraintsDirty => this.constraints is not null || this.balanceChanges is not null;
		private bool CommandsDirty => this.dbContextDirty || this.commands is not null;
		private bool Dirty => this.ConstraintsDirty || this.CommandsDirty;

		public void DeductCurrency(ICurrency currency, int amount)
		{
			this.balanceChanges ??= [];

			ref (ICurrency Currency, int Change) value = ref CollectionsMarshal.GetValueRefOrAddDefault(this.balanceChanges, this.SerializeCurrency(currency), out _);
			value.Currency = currency;
			value.Change -= amount;
		}

		public void AddBadge(IBadge badge)
		{
			if (this.transaction.user.Inventory.HasBadge(badge.Code))
			{
				return;
			}

			UserBadgeEntity entity = new()
			{
				UserId = this.transaction.user.Profile.Id,
				BadgeCode = badge.Code
			};

			this.dbContextDirty = true;
			this.badges ??= [];
			this.badges.Add(badge);
			this.transaction.dbContext.Add(entity);
		}

		public void AddFloorItem(IFloorFurniture furniture, JsonDocument? extraData)
		{
			Debug.Assert(this.transaction.furnitures.TryGetFloorFurniture(furniture.Id, out IFloorFurniture? debugFurniture) && debugFurniture == furniture);

			FloorItemEntity entity = new()
			{
				FurnitureId = furniture.Id,
				UserId = this.transaction.user.Profile.Id
			};

			if (extraData is not null)
			{
				entity.Data = new FloorItemDataEntity
				{
					ExtraData = extraData
				};
			}

			this.dbContextDirty = true;
			this.floorItems ??= [];
			this.floorItems.Add(entity);
			this.transaction.dbContext.Add(entity);
		}

		public void AddWallItem(IWallFurniture furniture, JsonDocument? extraData)
		{
			Debug.Assert(this.transaction.furnitures.TryGetWallFurniture(furniture.Id, out IWallFurniture? debugFurniture) && debugFurniture == furniture);

			WallItemEntity entity = new()
			{
				FurnitureId = furniture.Id,
				UserId = this.transaction.user.Profile.Id
			};

			if (extraData is not null)
			{
				entity.Data = new WallItemDataEntity
				{
					ExtraData = extraData
				};
			}

			this.dbContextDirty = true;
			this.wallItems ??= [];
			this.wallItems.Add(entity);
			this.transaction.dbContext.Add(entity);
		}

		public void AddConstraint(Func<ICatalogTransactionContext, DbTransaction, CancellationToken, ValueTask> action)
		{
			this.constraints ??= [];
			this.constraints.Add(action);
		}

		public void AddCommand(Func<ICatalogTransactionContext, DbTransaction, CancellationToken, ValueTask> action)
		{
			this.commands ??= [];
			this.commands.Add(action);
		}

		internal async Task<ICatalogTransactionResult> CompleteAsync(CancellationToken cancellationToken)
		{
			if (!this.Dirty)
			{
				return CatalogTransactionResult.Success;
			}

			try
			{
				do
				{
					ICatalogTransactionResult? constraintsResult = await this.ExecuteConstraints(cancellationToken).ConfigureAwait(false);
					if (constraintsResult is null)
					{
						await this.ExecuteCommands(cancellationToken).ConfigureAwait(false);
					}
					else
					{
						return constraintsResult;
					}
				}
				while (this.Dirty);

				await this.transaction.dbTransaction.CommitAsync(cancellationToken).ConfigureAwait(false);
			}
			catch
			{
				await this.transaction.dbTransaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
				throw;
			}

			return this.CreateSuccessResult();
		}

		private async Task<ICatalogTransactionResult?> ExecuteConstraints(CancellationToken cancellationToken)
		{
			while (this.ConstraintsDirty)
			{
				if (this.constraints is not null)
				{
					(List<Func<ICatalogTransactionContext, DbTransaction, CancellationToken, ValueTask>>? constraints, this.constraints) = (this.constraints, null);

					foreach (Func<ICatalogTransactionContext, DbTransaction, CancellationToken, ValueTask> constraint in constraints)
					{
						await constraint(this, this.transaction.dbTransaction.GetDbTransaction(), cancellationToken).ConfigureAwait(false);
					}
				}

				if (this.balanceChanges is not null)
				{
					(Dictionary<(string Key, string? Data), (ICurrency Currency, int Change)> balanceChanges, this.balanceChanges) = (this.balanceChanges, null);

					this.purse ??= [];

					int i = 0;
					await foreach (UserPurseEntity entity in this.transaction.dbContext.UserPurse
						.ToLinqToDBTable()
						.Merge()
						.Using(balanceChanges.Select(static b => (b.Key.Key, b.Key.Data, b.Value.Change)))
						.On((e, b) => e.UserId == this.transaction.user.Profile.Id && e.CurrencyType == b.Key && e.CurrencyData == b.Data)
						.UpdateWhenMatchedAnd(static (e, b) => e.Balance + b.Change >= 0, static (e, b) => new UserPurseEntity
						{
							Balance = e.Balance + b.Change
						})
						.MergeWithOutputAsync(static (_, inserted, _) => inserted)
						.WithCancellation(cancellationToken)
						.ConfigureAwait(false))
					{
						i++;

						ICurrency currency = balanceChanges[(entity.CurrencyType, entity.CurrencyData)].Currency;

						CollectionsMarshal.GetValueRefOrAddDefault(this.purse, currency, out _) = entity.Balance;
					}

					if (balanceChanges.Count != i)
					{
						return CatalogTransactionResult.InsufficientFunds;
					}
				}
			}

			return null;
		}

		private async Task ExecuteCommands(CancellationToken cancellationToken)
		{
			while (!this.ConstraintsDirty && this.CommandsDirty)
			{
				if (this.commands is not null)
				{
					(List<Func<ICatalogTransactionContext, DbTransaction, CancellationToken, ValueTask>>? commands, this.commands) = (this.commands, null);
					foreach (Func<ICatalogTransactionContext, DbTransaction, CancellationToken, ValueTask> command in commands)
					{
						await command(this, this.transaction.dbTransaction.GetDbTransaction(), cancellationToken).ConfigureAwait(false);
					}
				}

				if (this.dbContextDirty)
				{
					this.dbContextDirty = false;

					await this.transaction.dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
				}
			}
		}

		private ICatalogTransactionResult CreateSuccessResult()
		{
			List<IInventoryItem> items = [];
			if (this.badges is not null)
			{
				foreach (IBadge badge in this.badges)
				{
					items.Add(new BadgeInventoryItem(badge, this.transaction.user.Profile));
				}
			}

			if (this.floorItems is not null)
			{
				foreach (FloorItemEntity item in this.floorItems)
				{
					this.transaction.furnitures.TryGetFloorFurniture(item.FurnitureId, out IFloorFurniture? furniture);

					items.Add(this.transaction.furnitureInventoryItemStrategy.CreateFurnitureItem(item.Id, this.transaction.user.Profile, furniture!, item.Data?.ExtraData));
				}
			}

			if (this.wallItems is not null)
			{
				foreach (WallItemEntity item in this.wallItems)
				{
					this.transaction.furnitures.TryGetWallFurniture(item.FurnitureId, out IWallFurniture? furniture);

					items.Add(this.transaction.furnitureInventoryItemStrategy.CreateFurnitureItem(item.Id, this.transaction.user.Profile, furniture!, item.Data?.ExtraData));
				}
			}

			return new CatalogTransactionResult(ICatalogTransactionResult.ResultType.Success, this.purse ?? [], items);
		}

		internal async ValueTask DisposeAsync()
		{
			await this.transaction.dbContext.DisposeAsync().ConfigureAwait(false);
			await this.transaction.dbTransaction.DisposeAsync().ConfigureAwait(false);
		}

		private (string Key, string? Data) SerializeCurrency(ICurrency currency)
		{
			string key = this.transaction.currencyRegistry.Key(currency.Type).ToString();

			return currency.Data is { } data
				? (key, JsonSerializer.Serialize(data))
				: (key, null);
		}

		IUserInfo ICatalogTransactionContext.User => this.transaction.user.Profile;
		string ICatalogTransactionContext.ExtraData => this.transaction.ExtraData;

		public ICatalogTransactionContext.IConstraints Constraints => this;
		public ICatalogTransactionContext.ICommands Commands => this;
	}
}
