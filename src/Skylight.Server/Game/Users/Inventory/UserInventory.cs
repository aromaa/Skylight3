using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Skylight.API.Game.Badges;
using Skylight.API.Game.Furniture;
using Skylight.API.Game.Furniture.Floor;
using Skylight.API.Game.Furniture.Wall;
using Skylight.API.Game.Inventory;
using Skylight.API.Game.Inventory.Badges;
using Skylight.API.Game.Inventory.Items;
using Skylight.API.Game.Inventory.Items.Floor;
using Skylight.API.Game.Inventory.Items.Wall;
using Skylight.Domain.Badges;
using Skylight.Domain.Items;
using Skylight.Infrastructure;
using Skylight.Protocol.Packets.Data.Inventory.Furni;
using Skylight.Protocol.Packets.Data.Notifications;
using Skylight.Protocol.Packets.Data.Room.Object;
using Skylight.Protocol.Packets.Outgoing.Inventory.Badges;
using Skylight.Protocol.Packets.Outgoing.Inventory.Furni;
using Skylight.Protocol.Packets.Outgoing.Notifications;
using Skylight.Server.Extensions;
using Skylight.Server.Game.Inventory.Items.Badges;
using Skylight.Server.Game.Users.Authentication;

namespace Skylight.Server.Game.Users.Inventory;

internal sealed class UserInventory : IInventory
{
	private readonly User user;

	private readonly ConcurrentDictionary<string, IBadgeInventoryItem> badges;

	private readonly ConcurrentDictionary<int, IFloorInventoryItem> floorItems;
	private readonly ConcurrentDictionary<int, IWallInventoryItem> wallItems;

	internal UserInventory(User user)
	{
		this.user = user;

		this.badges = new ConcurrentDictionary<string, IBadgeInventoryItem>();

		this.floorItems = new ConcurrentDictionary<int, IFloorInventoryItem>();
		this.wallItems = new ConcurrentDictionary<int, IWallInventoryItem>();
	}

	public IEnumerable<IBadgeInventoryItem> Badges => this.badges.Values;

	public IEnumerable<IFloorInventoryItem> FloorItems => this.floorItems.Values;
	public IEnumerable<IWallInventoryItem> WallItems => this.wallItems.Values;

	internal async Task LoadAsync(SkylightContext dbContext, UserAuthentication.LoadContext loadContext, CancellationToken cancellationToken)
	{
		await this.LoadFurniAsync(dbContext, loadContext, cancellationToken).ConfigureAwait(false);
		await this.LoadBadges(dbContext, loadContext, cancellationToken).ConfigureAwait(false);
	}

	private async Task LoadFurniAsync(SkylightContext dbContext, UserAuthentication.LoadContext loadContext, CancellationToken cancellationToken)
	{
		IFurnitureSnapshot furnitures = await loadContext.FurnitureManager.GetAsync().ConfigureAwait(false);

		await foreach (FloorItemEntity item in dbContext.FloorItems
						   .AsNoTracking()
						   .Where(i => i.UserId == this.user.Id && i.RoomId == null)
						   .Include(i => i.Data)
						   .AsAsyncEnumerable()
						   .WithCancellation(cancellationToken)
						   .ConfigureAwait(false))
		{
			if (!furnitures.TryGetFloorFurniture(item.FurnitureId, out IFloorFurniture? furniture))
			{
				continue;
			}

			this.floorItems.TryAdd(item.Id, loadContext.FurnitureInventoryItemFactory.CreateFurnitureItem(item.Id, this.user.Info, furniture, item.Data?.ExtraData));
		}

		await foreach (WallItemEntity item in dbContext.WallItems
						   .AsNoTracking()
						   .Where(i => i.UserId == this.user.Id && i.RoomId == null)
						   .Include(i => i.Data)
						   .AsAsyncEnumerable()
						   .WithCancellation(cancellationToken)
						   .ConfigureAwait(false))
		{
			if (!furnitures.TryGetWallFurniture(item.FurnitureId, out IWallFurniture? furniture))
			{
				continue;
			}

			this.wallItems.TryAdd(item.Id, loadContext.FurnitureInventoryItemFactory.CreateFurnitureItem(item.Id, this.user.Info, furniture, item.Data?.ExtraData));
		}
	}

	private async Task LoadBadges(SkylightContext dbContext, UserAuthentication.LoadContext loadContext, CancellationToken cancellationToken)
	{
		IBadgeSnapshot badges = await loadContext.BadgeManager.GetAsync().ConfigureAwait(false);

		await foreach (UserBadgeEntity entity in dbContext.UserBadges
						   .AsNoTracking()
						   .Where(b => b.UserId == this.user.Id)
						   .AsAsyncEnumerable()
						   .WithCancellation(cancellationToken)
						   .ConfigureAwait(false))
		{
			if (!badges.TryGetBadge(entity.BadgeCode, out IBadge? badge))
			{
				continue;
			}

			this.badges.TryAdd(badge.Code, new BadgeInventoryItem(badge, this.user.Info));
		}
	}

	public void RefreshFurniture()
	{
		//TODO: Refresh from DB
	}

	public bool TryAddBadge(IBadgeInventoryItem badge)
	{
		if (this.badges.TryAdd(badge.Badge.Code, badge))
		{
			this.user.SendAsync(new BadgeReceivedOutgoingPacket(badge.Badge.Id, badge.Badge.Code));

			return true;
		}

		return false;
	}

	public bool TryAddFloorItem(IFloorInventoryItem item)
	{
		if (this.floorItems.TryAdd(item.Id, item))
		{
			this.user.SendAsync(new FurniListAddOrUpdateOutgoingPacket(
			[
				new InventoryItemData(item.StripId, item.Id, item.Furniture.Id, FurnitureType.Floor, 1, item.GetItemData())
			]));

			return true;
		}

		return false;
	}

	public bool TryAddWallItem(IWallInventoryItem item)
	{
		if (this.wallItems.TryAdd(item.Id, item))
		{
			this.user.SendAsync(new FurniListAddOrUpdateOutgoingPacket(
			[
				new InventoryItemData(item.StripId, item.Id, item.Furniture.Id, FurnitureType.Wall, 1, item.GetItemData())
			]));

			return true;
		}

		return false;
	}

	public void AddUnseenItems(IEnumerable<IInventoryItem> items)
	{
		List<int>? badgeIds = null;
		List<int>? furnitureIds = null;

		foreach (IInventoryItem item in items)
		{
			if (item is IFloorInventoryItem floorItem)
			{
				this.TryAddFloorItem(floorItem);

				furnitureIds ??= [];
				furnitureIds.Add(floorItem.StripId);
			}
			else if (item is IWallInventoryItem wallItem)
			{
				this.TryAddWallItem(wallItem);

				furnitureIds ??= [];
				furnitureIds.Add(wallItem.StripId);
			}
			else if (item is IBadgeInventoryItem badgeItem)
			{
				this.TryAddBadge(badgeItem);

				badgeIds ??= [];
				badgeIds.Add(badgeItem.Badge.Id);
			}
		}

		this.user.SendAsync(new UnseenItemsOutgoingPacket(
		[
			new UnseenItemData(1, furnitureIds is not null ? furnitureIds : Array.Empty<int>()),
			new UnseenItemData(4, badgeIds is not null ? badgeIds : Array.Empty<int>())
		]));
	}

	public void AddUnseenFloorItem(IFloorInventoryItem item)
	{
		this.TryAddFloorItem(item);

		this.user.SendAsync(new UnseenItemsOutgoingPacket(
		[
			new UnseenItemData(1, [item.StripId])
		]));
	}

	public bool TryRemoveFloorItem(IFloorInventoryItem item)
	{
		if (this.floorItems.TryRemove(item.Id, out _))
		{
			this.user.SendAsync(new FurniListRemoveOutgoingPacket(item.StripId));

			return true;
		}

		return false;
	}

	public bool TryRemoveWallItem(IWallInventoryItem item)
	{
		if (this.wallItems.TryRemove(item.Id, out _))
		{
			this.user.SendAsync(new FurniListRemoveOutgoingPacket(item.StripId));

			return true;
		}

		return false;
	}

	public bool TryRemoveFurniture(IFurnitureInventoryItem item)
	{
		if (item is IFloorInventoryItem floorItem)
		{
			return this.TryRemoveFloorItem(floorItem);
		}
		else if (item is IWallInventoryItem wallItem)
		{
			return this.TryRemoveWallItem(wallItem);
		}
		else
		{
			throw new ArgumentException($"Unknown item type {item.GetType()}", nameof(item));
		}
	}

	public bool HasBadge(string badgeCode) => this.badges.ContainsKey(badgeCode);

	public bool TryGetBadge(string badgeCode, [NotNullWhen(true)] out IBadgeInventoryItem? badge) => this.badges.TryGetValue(badgeCode, out badge);

	public bool TryGetFloorItem(int id, [NotNullWhen(true)] out IFloorInventoryItem? item) => this.floorItems.TryGetValue(id, out item);
	public bool TryGetWallItem(int id, [NotNullWhen(true)] out IWallInventoryItem? item) => this.wallItems.TryGetValue(id, out item);
	public bool TryGetFurnitureItem(int stripId, [NotNullWhen(true)] out IFurnitureInventoryItem? item)
	{
		if (stripId < 0)
		{
			if (this.TryGetFloorItem(-stripId, out IFloorInventoryItem? floorItem))
			{
				item = floorItem;
				return true;
			}
		}
		else
		{
			if (this.TryGetWallItem(stripId, out IWallInventoryItem? wallItem))
			{
				item = wallItem;
				return true;
			}
		}

		item = null;
		return false;
	}
}
