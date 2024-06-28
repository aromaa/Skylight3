using Microsoft.EntityFrameworkCore;
using Net.Communication.Attributes;
using Skylight.API.Game.Inventory.Items;
using Skylight.API.Game.Rooms.Items.Floor;
using Skylight.API.Game.Rooms.Items.Wall;
using Skylight.API.Game.Rooms.Units;
using Skylight.API.Game.Users;
using Skylight.Infrastructure;
using Skylight.Protocol.Packets.Incoming.Room.Engine;
using Skylight.Protocol.Packets.Manager;

namespace Skylight.Server.Game.Communication.Room.Engine;

[PacketManagerRegister(typeof(AbstractGamePacketManager))]
internal sealed partial class PickupObjectPacketHandler<T>(IDbContextFactory<SkylightContext> dbContextFactory, IFurnitureInventoryItemStrategy furnitureInventoryItemFactory)
	: UserPacketHandler<T>
	where T : IPickupObjectIncomingPacket
{
	private readonly IDbContextFactory<SkylightContext> dbContextFactory = dbContextFactory;

	private readonly IFurnitureInventoryItemStrategy furnitureInventoryItemFactory = furnitureInventoryItemFactory;

	internal override void Handle(IUser user, in T packet)
	{
		if (user.RoomSession?.Unit is not { } roomUnit)
		{
			return;
		}

		if (packet.ItemType == 1)
		{
			int floorItemId = packet.ItemId;

			this.PickupWallItem(roomUnit, floorItemId);
		}
		else if (packet.ItemType == 2)
		{
			int wallItemId = packet.ItemId;

			this.PickupFloorItem(roomUnit, wallItemId);
		}
	}

	private void PickupFloorItem(IUserRoomUnit roomUnit, int floorItemId)
	{
		roomUnit.User.Client.ScheduleTask(async _ =>
		{
			IFloorRoomItem? item = await roomUnit.Room.ScheduleTask(room =>
			{
				if (!roomUnit.InRoom || !room.ItemManager.TryGetFloorItem(floorItemId, out IFloorRoomItem? item) || !room.ItemManager.CanPickupItem(item, roomUnit.User))
				{
					return default;
				}

				room.ItemManager.RemoveItem(item);

				roomUnit.User.Inventory.TryAddFloorItem(this.furnitureInventoryItemFactory.CreateFurnitureItem(item.Id, item.Owner, item.Furniture, null));

				return item;
			}).ConfigureAwait(false);

			if (item is null)
			{
				return;
			}

			await using SkylightContext dbContext = await this.dbContextFactory.CreateDbContextAsync().ConfigureAwait(false);

			await dbContext.FloorItems
				.Where(i => i.Id == item.Id)
				.ExecuteUpdateAsync(setters => setters
					.SetProperty(i => i.RoomId, (int?)null)
					.SetProperty(i => i.UserId, roomUnit.User.Profile.Id))
				.ConfigureAwait(false);
		});
	}

	private void PickupWallItem(IUserRoomUnit roomUnit, int wallItemId)
	{
		roomUnit.User.Client.ScheduleTask(async _ =>
		{
			IWallRoomItem? item = await roomUnit.Room.ScheduleTask(room =>
			{
				if (!roomUnit.InRoom || !room.ItemManager.TryGetWallItem(wallItemId, out IWallRoomItem? item) || !room.ItemManager.CanPickupItem(item, roomUnit.User))
				{
					return default;
				}

				room.ItemManager.RemoveItem(item);

				roomUnit.User.Inventory.TryAddWallItem(this.furnitureInventoryItemFactory.CreateFurnitureItem(item.Id, item.Owner, item.Furniture, null));

				return item;
			}).ConfigureAwait(false);

			if (item is null)
			{
				return;
			}

			await using SkylightContext dbContext = await this.dbContextFactory.CreateDbContextAsync().ConfigureAwait(false);

			await dbContext.WallItems
				.Where(i => i.Id == item.Id)
				.ExecuteUpdateAsync(setters => setters
					.SetProperty(i => i.RoomId, (int?)null)
					.SetProperty(i => i.UserId, roomUnit.User.Profile.Id))
				.ConfigureAwait(false);
		});
	}
}
