using Microsoft.EntityFrameworkCore;
using Net.Communication.Attributes;
using Skylight.API.Game.Inventory.Items;
using Skylight.API.Game.Rooms.Items;
using Skylight.API.Game.Rooms.Items.Floor;
using Skylight.API.Game.Rooms.Items.Wall;
using Skylight.API.Game.Rooms.Private;
using Skylight.API.Game.Rooms.Units;
using Skylight.API.Game.Users;
using Skylight.API.Registry;
using Skylight.Infrastructure;
using Skylight.Protocol.Packets.Incoming.Room.Engine;
using Skylight.Protocol.Packets.Manager;

namespace Skylight.Server.Game.Communication.Room.Engine;

[PacketManagerRegister(typeof(IGamePacketManager))]
internal sealed partial class PickupObjectPacketHandler<T>(IRegistryHolder registryHolder, IDbContextFactory<SkylightContext> dbContextFactory, IFurnitureInventoryItemStrategy furnitureInventoryItemFactory)
	: UserPacketHandler<T>
	where T : IPickupObjectIncomingPacket
{
	// TODO: Support other domains
	private readonly IRoomItemDomain normalRoomItemDomain = RoomItemDomains.Normal.Get(registryHolder);

	private readonly IDbContextFactory<SkylightContext> dbContextFactory = dbContextFactory;

	private readonly IFurnitureInventoryItemStrategy furnitureInventoryItemFactory = furnitureInventoryItemFactory;

	internal override void Handle(IUser user, in T packet)
	{
		if (user.RoomSession?.Unit is not { Room: IPrivateRoom privateRoom } roomUnit)
		{
			return;
		}

		if (packet.ItemType == 1)
		{
			this.PickupWallItem(privateRoom, roomUnit, new RoomItemId(this.normalRoomItemDomain, packet.ItemId));
		}
		else if (packet.ItemType == 2)
		{
			this.PickupFloorItem(privateRoom, roomUnit, new RoomItemId(this.normalRoomItemDomain, packet.ItemId));
		}
	}

	private void PickupFloorItem(IPrivateRoom privateRoom, IUserRoomUnit roomUnit, RoomItemId floorItemId)
	{
		roomUnit.User.Client.ScheduleTask(async _ =>
		{
			IFloorRoomItem? item = await roomUnit.Room.ScheduleTask(_ =>
			{
				if (!roomUnit.InRoom || !privateRoom.ItemManager.TryGetFloorItem(floorItemId, out IFloorRoomItem? item) || !privateRoom.ItemManager.CanPickupItem(item, roomUnit.User))
				{
					return default;
				}

				privateRoom.ItemManager.RemoveItem(item);

				return item;
			}).ConfigureAwait(false);

			if (item is null || floorItemId.Domain != this.normalRoomItemDomain)
			{
				return;
			}

			roomUnit.User.Inventory.TryAddFloorItem(this.furnitureInventoryItemFactory.CreateFurnitureItem(item.Id.Id, item.Owner, item.Furniture));

			await using SkylightContext dbContext = await this.dbContextFactory.CreateDbContextAsync().ConfigureAwait(false);

			await dbContext.FloorItems
				.Where(i => i.Id == item.Id.Id)
				.ExecuteUpdateAsync(setters => setters
					.SetProperty(i => i.RoomId, (int?)null)
					.SetProperty(i => i.UserId, roomUnit.User.Profile.Id))
				.ConfigureAwait(false);
		});
	}

	private void PickupWallItem(IPrivateRoom privateRoom, IUserRoomUnit roomUnit, RoomItemId wallItemId)
	{
		roomUnit.User.Client.ScheduleTask(async _ =>
		{
			IWallRoomItem? item = await roomUnit.Room.ScheduleTask(_ =>
			{
				if (!roomUnit.InRoom || !privateRoom.ItemManager.TryGetWallItem(wallItemId, out IWallRoomItem? item) || !privateRoom.ItemManager.CanPickupItem(item, roomUnit.User))
				{
					return default;
				}

				privateRoom.ItemManager.RemoveItem(item);

				return item;
			}).ConfigureAwait(false);

			if (item is null || wallItemId.Domain != this.normalRoomItemDomain)
			{
				return;
			}

			roomUnit.User.Inventory.TryAddWallItem(this.furnitureInventoryItemFactory.CreateFurnitureItem(item.Id.Id, item.Owner, item.Furniture));

			await using SkylightContext dbContext = await this.dbContextFactory.CreateDbContextAsync().ConfigureAwait(false);

			await dbContext.WallItems
				.Where(i => i.Id == item.Id.Id)
				.ExecuteUpdateAsync(setters => setters
					.SetProperty(i => i.RoomId, (int?)null)
					.SetProperty(i => i.UserId, roomUnit.User.Profile.Id))
				.ConfigureAwait(false);
		});
	}
}
