using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Skylight.API.Game.Furniture.Wall;
using Skylight.API.Game.Inventory.Items.Wall;
using Skylight.API.Game.Users;
using Skylight.Domain.Items;
using Skylight.Infrastructure;

namespace Skylight.Server.Game.Inventory.Items.Wall;

internal sealed class StickyNoteInventoryItem : WallInventoryItem, IStickyNoteInventoryItem
{
	private readonly IDbContextFactory<SkylightContext> dbContextFactory;

	public override IStickyNoteFurniture Furniture { get; }

	public int Count { get; private set; }

	internal StickyNoteInventoryItem(IDbContextFactory<SkylightContext> dbContextFactory, int id, IUserInfo owner, IStickyNoteFurniture furniture, int count)
		: base(id, owner)
	{
		this.dbContextFactory = dbContextFactory;

		this.Furniture = furniture;

		this.Count = count;
	}

	public async Task<IStickyNoteInventoryItem?> TryConsumeAsync(int roomId, CancellationToken cancellationToken = default)
	{
		if (this.Count == 0)
		{
			return null;
		}

		int itemId = this.Id;
		int userId = this.Owner.Id;

		if (this.Count == 1)
		{
			await using (SkylightContext dbContext = await this.dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
			{
				int count = await dbContext.WallItems.Where(i => i.Id == itemId && i.UserId == userId && i.RoomId == null)
					.ExecuteUpdateAsync(setters => setters
						.SetProperty(i => i.RoomId, roomId)
						.SetProperty(i => i.LocationX, -1)
						.SetProperty(i => i.LocationY, -1), cancellationToken)
					.ConfigureAwait(false);

				if (count == 0)
				{
					return null;
				}
			}

			return this;
		}
		else
		{
			await using SkylightContext dbContext = await this.dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
			await using IDbContextTransaction transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);

			int count = await dbContext.WallItems
				.Where(i => i.Id == this.Id && i.ExtraData!.RootElement.GetInt32() > 1)
				.ExecuteUpdateAsync(setters =>
					setters.SetProperty(i => i.ExtraData, i => (JsonDocument)(object)(string)(object)(i.ExtraData!.RootElement.GetInt32() - 1)), cancellationToken)
				.ConfigureAwait(false);

			if (count == 0)
			{
				throw new DbUpdateConcurrencyException();
			}

			WallItemEntity entity = new()
			{
				UserId = this.Owner.Id,
				FurnitureId = this.Furniture.Id,
				RoomId = roomId,

				LocationX = -1,
				LocationY = -1,

				ExtraData = JsonSerializer.SerializeToDocument(1)
			};

			dbContext.WallItems.Add(entity);

			await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
			await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);

			this.Count--;

			return new StickyNoteInventoryItem(this.dbContextFactory, entity.Id, this.Owner, this.Furniture, 1);
		}
	}

	public JsonDocument AsExtraData() => JsonSerializer.SerializeToDocument(this.Count);
}
