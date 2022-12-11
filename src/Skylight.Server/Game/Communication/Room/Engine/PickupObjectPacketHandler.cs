using System.Runtime.InteropServices;
using Microsoft.EntityFrameworkCore;
using Net.Communication.Attributes;
using Skylight.API.Game.Clients;
using Skylight.API.Game.Inventory.Items;
using Skylight.API.Game.Rooms;
using Skylight.API.Game.Rooms.Items.Floor;
using Skylight.API.Game.Rooms.Items.Wall;
using Skylight.API.Game.Rooms.Units;
using Skylight.API.Game.Users;
using Skylight.Domain.Items;
using Skylight.Infrastructure;
using Skylight.Protocol.Packets.Incoming.Room.Engine;
using Skylight.Protocol.Packets.Manager;

namespace Skylight.Server.Game.Communication.Room.Engine;

[PacketManagerRegister(typeof(AbstractGamePacketManager))]
internal sealed class PickupObjectPacketHandler<T> : UserPacketHandler<T>
	where T : IPickupObjectIncomingPacket
{
	private readonly IDbContextFactory<SkylightContext> dbContextFactory;

	private readonly IFurnitureInventoryItemStrategy furnitureInventoryItemFactory;

	public PickupObjectPacketHandler(IDbContextFactory<SkylightContext> dbContextFactory, IFurnitureInventoryItemStrategy furnitureInventoryItemFactory)
	{
		this.dbContextFactory = dbContextFactory;

		this.furnitureInventoryItemFactory = furnitureInventoryItemFactory;
	}

	internal override void Handle(IUser user, in T packet)
	{
		if (user.RoomSession?.Unit is not { } roomUnit)
		{
			return;
		}

		if (packet.ItemType == 1)
		{
			user.Client.ScheduleTask(new PickupWallItemTask
			{
				DbContextFactory = this.dbContextFactory,

				FurnitureInventoryItemFactory = this.furnitureInventoryItemFactory,

				RoomUnit = roomUnit,

				ItemId = packet.ItemId
			});
		}
		else if (packet.ItemType == 2)
		{
			user.Client.ScheduleTask(new PickupFloorItemTask
			{
				DbContextFactory = this.dbContextFactory,

				FurnitureInventoryItemFactory = this.furnitureInventoryItemFactory,

				RoomUnit = roomUnit,

				ItemId = packet.ItemId
			});
		}
	}

	[StructLayout(LayoutKind.Auto)]
	private readonly struct PickupFloorItemTask : IClientTask, IRoomTask<IFloorRoomItem?>
	{
		internal IDbContextFactory<SkylightContext> DbContextFactory { get; init; }

		internal IFurnitureInventoryItemStrategy FurnitureInventoryItemFactory { get; init; }

		internal IUserRoomUnit RoomUnit { get; init; }

		internal int ItemId { get; init; }

		public async Task ExecuteAsync(IClient client)
		{
			IFloorRoomItem? item = await this.RoomUnit.Room.ScheduleTaskAsync<PickupFloorItemTask, IFloorRoomItem?>(this).ConfigureAwait(false);
			if (item is null)
			{
				return;
			}

			FloorItemEntity floorItem = new()
			{
				Id = item.Id,
				UserId = item.Owner.Id,
				RoomId = this.RoomUnit.Room.Info.Id
			};

			await using SkylightContext dbContext = await this.DbContextFactory.CreateDbContextAsync().ConfigureAwait(false);

			dbContext.FloorItems.Attach(floorItem);

			floorItem.RoomId = null;

			await dbContext.SaveChangesAsync().ConfigureAwait(false);
		}

		public IFloorRoomItem? Execute(IRoom room)
		{
			if (!this.RoomUnit.InRoom || !room.ItemManager.TryGetFloorItem(this.ItemId, out IFloorRoomItem? item))
			{
				return null;
			}

			room.ItemManager.RemoveItem(item);

			this.RoomUnit.User.Inventory.TryAddFloorItem(this.FurnitureInventoryItemFactory.CreateFurnitureItem(item.Id, item.Owner, item.Furniture, null));

			return item;
		}
	}

	[StructLayout(LayoutKind.Auto)]
	private readonly struct PickupWallItemTask : IClientTask, IRoomTask<IWallRoomItem?>
	{
		internal IDbContextFactory<SkylightContext> DbContextFactory { get; init; }

		internal IFurnitureInventoryItemStrategy FurnitureInventoryItemFactory { get; init; }

		internal IUserRoomUnit RoomUnit { get; init; }

		internal int ItemId { get; init; }

		public async Task ExecuteAsync(IClient client)
		{
			IWallRoomItem? item = await this.RoomUnit.Room.ScheduleTaskAsync<PickupWallItemTask, IWallRoomItem?>(this).ConfigureAwait(false);
			if (item is null)
			{
				return;
			}

			WallItemEntity wallItem = new()
			{
				Id = item.Id,
				UserId = item.Owner.Id,
				RoomId = this.RoomUnit.Room.Info.Id
			};

			await using SkylightContext dbContext = await this.DbContextFactory.CreateDbContextAsync().ConfigureAwait(false);

			dbContext.WallItems.Attach(wallItem);

			wallItem.RoomId = null;

			await dbContext.SaveChangesAsync().ConfigureAwait(false);
		}

		public IWallRoomItem? Execute(IRoom room)
		{
			if (!this.RoomUnit.InRoom || !room.ItemManager.TryGetWallItem(this.ItemId, out IWallRoomItem? item))
			{
				return null;
			}

			room.ItemManager.RemoveItem(item);

			this.RoomUnit.User.Inventory.TryAddWallItem(this.FurnitureInventoryItemFactory.CreateFurnitureItem(item.Id, item.Owner, item.Furniture, null));

			return item;
		}
	}
}
