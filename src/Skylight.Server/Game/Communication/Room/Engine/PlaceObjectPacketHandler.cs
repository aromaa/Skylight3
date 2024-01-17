using System.Buffers;
using System.Buffers.Text;
using Microsoft.EntityFrameworkCore;
using Net.Communication.Attributes;
using Skylight.API.Game.Inventory.Items;
using Skylight.API.Game.Inventory.Items.Floor;
using Skylight.API.Game.Inventory.Items.Wall;
using Skylight.API.Game.Rooms.Items;
using Skylight.API.Game.Rooms.Items.Floor;
using Skylight.API.Game.Rooms.Items.Wall;
using Skylight.API.Game.Rooms.Units;
using Skylight.API.Game.Users;
using Skylight.API.Numerics;
using Skylight.Domain.Items;
using Skylight.Infrastructure;
using Skylight.Protocol.Packets.Incoming.Room.Engine;
using Skylight.Protocol.Packets.Manager;

namespace Skylight.Server.Game.Communication.Room.Engine;

[PacketManagerRegister(typeof(AbstractGamePacketManager))]
internal sealed partial class PlaceObjectPacketHandler<T> : UserPacketHandler<T>
	where T : IPlaceObjectIncomingPacket
{
	private readonly IDbContextFactory<SkylightContext> dbContextFactory;

	private readonly IFloorRoomItemStrategy floorRoomItemStrategy;
	private readonly IWallRoomItemStrategy wallRoomItemStrategy;

	public PlaceObjectPacketHandler(IDbContextFactory<SkylightContext> dbContextFactory, IFloorRoomItemStrategy floorRoomItemStrategy, IWallRoomItemStrategy wallRoomRoomItemStrategy)
	{
		this.dbContextFactory = dbContextFactory;

		this.floorRoomItemStrategy = floorRoomItemStrategy;
		this.wallRoomItemStrategy = wallRoomRoomItemStrategy;
	}

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
			Point3D? position = await roomUnit.Room.ScheduleTask(room =>
			{
				Point3D position = new(location, room.ItemManager.GetPlacementHeight(floorItem.Furniture, location));

				if (!roomUnit.InRoom || !room.ItemManager.CanPlaceItem(floorItem.Furniture, position)
										   || !roomUnit.User.Inventory.TryRemoveFloorItem(floorItem))
				{
					return default(Point3D?);
				}

				IFloorRoomItem item = this.floorRoomItemStrategy.CreateFloorItem(room, floorItem, position, direction);

				room.ItemManager.AddItem(item);

				return position;
			}).ConfigureAwait(false);

			if (position is null)
			{
				return;
			}

			await using SkylightContext dbContext = await this.dbContextFactory.CreateDbContextAsync().ConfigureAwait(false);

			FloorItemEntity item = new() { Id = floorItem.Id, UserId = floorItem.Owner.Id };

			dbContext.FloorItems.Attach(item);

			item.RoomId = roomUnit.Room.Info.Id;
			item.X = position.Value.X;
			item.Y = position.Value.Y;
			item.Z = position.Value.Z;

			await dbContext.SaveChangesAsync().ConfigureAwait(false);
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
				(Point2D Location, Point2D Position)? result = await roomUnit.Room.ScheduleTask(room =>
				{
					if (!roomUnit.InRoom || !room.ItemManager.CanPlaceItem(wallItem.Furniture, location, position)
											   || !roomUnit.User.Inventory.TryRemoveWallItem(wallItem))
					{
						return default;
					}

					IWallRoomItem item = this.wallRoomItemStrategy.CreateWallItem(room, wallItem, location, position);

					room.ItemManager.AddItem(item);

					return (item.Location, item.Position);
				}).ConfigureAwait(false);

				if (result is null)
				{
					return;
				}

				await using SkylightContext dbContext = await this.dbContextFactory.CreateDbContextAsync().ConfigureAwait(false);

				WallItemEntity item = new()
				{
					Id = wallItem.Id,
					UserId = wallItem.Owner.Id
				};

				dbContext.WallItems.Attach(item);

				item.RoomId = roomUnit.Room.Info.Id;
				item.LocationX = result.Value.Location.X;
				item.LocationY = result.Value.Location.Y;
				item.PositionX = result.Value.Position.X;
				item.PositionY = result.Value.Position.Y;

				await dbContext.SaveChangesAsync().ConfigureAwait(false);
			});
		}
	}
}
