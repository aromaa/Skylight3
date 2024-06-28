using System.Buffers;
using System.Buffers.Text;
using Microsoft.EntityFrameworkCore;
using Net.Communication.Attributes;
using Skylight.API.Game.Inventory.Items;
using Skylight.API.Game.Inventory.Items.Floor;
using Skylight.API.Game.Inventory.Items.Wall;
using Skylight.API.Game.Rooms.Items.Floor;
using Skylight.API.Game.Rooms.Items.Wall;
using Skylight.API.Game.Rooms.Units;
using Skylight.API.Game.Users;
using Skylight.API.Numerics;
using Skylight.Infrastructure;
using Skylight.Protocol.Packets.Incoming.Room.Engine;
using Skylight.Protocol.Packets.Manager;
using Skylight.Server.Extensions;

namespace Skylight.Server.Game.Communication.Room.Engine;

[PacketManagerRegister(typeof(AbstractGamePacketManager))]
internal sealed partial class PlaceObjectPacketHandler<T>(IDbContextFactory<SkylightContext> dbContextFactory, IFloorRoomItemStrategy floorRoomItemStrategy, IWallRoomItemStrategy wallRoomRoomItemStrategy)
	: UserPacketHandler<T>
	where T : IPlaceObjectIncomingPacket
{
	private readonly IDbContextFactory<SkylightContext> dbContextFactory = dbContextFactory;

	private readonly IFloorRoomItemStrategy floorRoomItemStrategy = floorRoomItemStrategy;
	private readonly IWallRoomItemStrategy wallRoomItemStrategy = wallRoomRoomItemStrategy;

	internal override void Handle(IUser user, in T packet)
	{
		if (user.RoomSession?.Unit is not { } roomUnit)
		{
			return;
		}

		SequenceReader<byte> reader = new(packet.Values);

		if (!reader.TryReadTo(out ReadOnlySpan<byte> itemIdBuffer, (byte)' ')
			|| !Utf8Parser.TryParse(itemIdBuffer, out int stripId, out _))
		{
			return;
		}

		if (!user.Inventory.TryGetFurnitureItem(stripId, out IFurnitureInventoryItem? item))
		{
			return;
		}

		if (item is IFloorInventoryItem floorItem)
		{
			this.PlaceFloorItem(roomUnit, floorItem, reader);
		}
		else if (item is IWallInventoryItem wallItem)
		{
			if (wallItem is IStickyNoteInventoryItem)
			{
				return;
			}

			this.PlaceWallItem(roomUnit, wallItem, reader);
		}
	}

	private void PlaceFloorItem(IUserRoomUnit roomUnit, IFloorInventoryItem floorItem, SequenceReader<byte> reader)
	{
		if (!reader.TryReadTo(out ReadOnlySpan<byte> xBuffer, (byte)' ')
			|| !Utf8Parser.TryParse(xBuffer, out int x, out _))
		{
			return;
		}

		if (!reader.TryReadTo(out ReadOnlySpan<byte> yBuffer, (byte)' ')
			|| !Utf8Parser.TryParse(yBuffer, out int y, out _))
		{
			return;
		}

		if (!Utf8Parser.TryParse(reader.UnreadSequence.IsSingleSegment ? reader.UnreadSpan : reader.UnreadSequence.ToArray(), out int direction, out _))
		{
			return;
		}

		Point2D location = new(x, y);

		roomUnit.User.Client.ScheduleTask(async _ =>
		{
			bool canPlace = roomUnit.Room.ScheduleTask(room =>
			{
				Point3D position = new(location, room.ItemManager.GetPlacementHeight(floorItem.Furniture, location, direction));

				return roomUnit.InRoom && room.ItemManager.CanPlaceItem(floorItem.Furniture, position, direction, roomUnit.User) && roomUnit.User.Inventory.TryRemoveFloorItem(floorItem);
			}).TryGetOrSuppressThrowing(out bool canPlaceAwait, out ValueTaskExtensions.Awaiter<bool> canPlaceAwaiter) ? canPlaceAwait : await canPlaceAwaiter;

			if (!canPlace)
			{
				return;
			}

			int itemId = floorItem.Id;
			int userId = roomUnit.User.Profile.Id;
			int roomId = roomUnit.Room.Info.Id;

			await using (SkylightContext dbContext = await this.dbContextFactory.CreateDbContextAsync().ConfigureAwait(false))
			{
				int count = await dbContext.FloorItems
					.Where(i => i.Id == itemId && i.UserId == userId && i.RoomId == null)
					.ExecuteUpdateAsync(setters => setters
						.SetProperty(i => i.RoomId, roomId)
						.SetProperty(i => i.X, -1)
						.SetProperty(i => i.Y, -1))
					.ConfigureAwait(false);

				if (count == 0)
				{
					return;
				}
			}

			bool placed = roomUnit.Room.ScheduleTask(room =>
			{
				Point3D position = new(location, room.ItemManager.GetPlacementHeight(floorItem.Furniture, location, direction));
				if (!roomUnit.InRoom || !room.ItemManager.CanPlaceItem(floorItem.Furniture, position, direction, roomUnit.User))
				{
					return false;
				}

				room.ItemManager.AddItem(this.floorRoomItemStrategy.CreateFloorItem(floorItem, room, position, direction));

				return true;
			}).TryGetOrSuppressThrowing(out bool placeAwait, out ValueTaskExtensions.Awaiter<bool> placeAwaiter) ? placeAwait : await placeAwaiter;

			if (!placed)
			{
				await using (SkylightContext dbContext = await this.dbContextFactory.CreateDbContextAsync().ConfigureAwait(false))
				{
					int count = await dbContext.FloorItems
						.Where(i => i.Id == itemId && i.UserId == userId && i.RoomId == roomId && i.X == -1)
						.ExecuteUpdateAsync(setters =>
							setters.SetProperty(i => i.RoomId, (int?)null))
						.ConfigureAwait(false);

					if (count == 0)
					{
						return;
					}
				}

				roomUnit.User.Inventory.TryAddFloorItem(floorItem);
			}
		});
	}

	private void PlaceWallItem(IUserRoomUnit roomUnit, IWallInventoryItem wallItem, SequenceReader<byte> reader)
	{
		if (reader.IsNext((byte)':', advancePast: true))
		{
			if (!reader.IsNext("w="u8, advancePast: true)
				|| !reader.TryReadTo(out ReadOnlySpan<byte> locationXBuffer, (byte)',')
				|| !Utf8Parser.TryParse(locationXBuffer, out int xLocation, out _))
			{
				return;
			}

			if (!reader.TryReadTo(out ReadOnlySpan<byte> locationYBuffer, (byte)' ')
				|| !Utf8Parser.TryParse(locationYBuffer, out int yLocation, out _))
			{
				return;
			}

			if (!reader.IsNext("l="u8, advancePast: true)
				|| !reader.TryReadTo(out ReadOnlySpan<byte> positionXBuffer, (byte)',')
				|| !Utf8Parser.TryParse(positionXBuffer, out int xPosition, out _))
			{
				return;
			}

			if (!reader.TryReadTo(out ReadOnlySpan<byte> positionYBuffer, (byte)' ')
				|| !Utf8Parser.TryParse(positionYBuffer, out int yPosition, out _))
			{
				return;
			}

			if (!reader.TryRead(out byte direction))
			{
				return;
			}

			Point2D location = new(xLocation, yLocation);
			Point2D position = new(xPosition, yPosition);

			roomUnit.User.Client.ScheduleTask(async _ =>
			{
				bool canPlace = roomUnit.Room.ScheduleTask(room =>
						roomUnit.InRoom && room.ItemManager.CanPlaceItem(wallItem.Furniture, location, position, direction, roomUnit.User) && roomUnit.User.Inventory.TryRemoveWallItem(wallItem))
					.TryGetOrSuppressThrowing(out bool canPlaceAwait, out ValueTaskExtensions.Awaiter<bool> canPlaceAwaiter) ? canPlaceAwait : await canPlaceAwaiter;

				if (!canPlace)
				{
					return;
				}

				int itemId = wallItem.Id;
				int userId = roomUnit.User.Profile.Id;
				int roomId = roomUnit.Room.Info.Id;

				await using (SkylightContext dbContext = await this.dbContextFactory.CreateDbContextAsync().ConfigureAwait(false))
				{
					int count = await dbContext.WallItems
						.Where(i => i.Id == itemId && i.UserId == userId && i.RoomId == null)
						.ExecuteUpdateAsync(setters => setters
							.SetProperty(i => i.RoomId, roomId)
							.SetProperty(i => i.LocationX, -1)
							.SetProperty(i => i.LocationY, -1))
						.ConfigureAwait(false);

					if (count == 0)
					{
						return;
					}
				}

				bool placed = roomUnit.Room.ScheduleTask(room =>
				{
					if (!roomUnit.InRoom || !room.ItemManager.CanPlaceItem(wallItem.Furniture, location, position, direction, roomUnit.User))
					{
						return false;
					}

					room.ItemManager.AddItem(this.wallRoomItemStrategy.CreateWallItem(wallItem, room, location, position));

					return true;
				}).TryGetOrSuppressThrowing(out bool placeAwait, out ValueTaskExtensions.Awaiter<bool> placeAwaiter) ? placeAwait : await placeAwaiter;

				if (!placed)
				{
					await using (SkylightContext dbContext = await this.dbContextFactory.CreateDbContextAsync().ConfigureAwait(false))
					{
						int count = await dbContext.WallItems
							.Where(i => i.Id == itemId && i.UserId == userId && i.RoomId == roomId && i.LocationX == -1)
							.ExecuteUpdateAsync(setters =>
								setters.SetProperty(i => i.RoomId, (int?)null))
							.ConfigureAwait(false);

						if (count == 0)
						{
							return;
						}
					}

					roomUnit.User.Inventory.TryAddWallItem(wallItem);
				}
			});
		}
	}
}
