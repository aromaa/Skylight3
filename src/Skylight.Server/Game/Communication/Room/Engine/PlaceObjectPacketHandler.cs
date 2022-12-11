using System.Buffers;
using System.Buffers.Text;
using System.Runtime.InteropServices;
using Microsoft.EntityFrameworkCore;
using Net.Communication.Attributes;
using Skylight.API.Game.Clients;
using Skylight.API.Game.Inventory.Items;
using Skylight.API.Game.Inventory.Items.Floor;
using Skylight.API.Game.Inventory.Items.Wall;
using Skylight.API.Game.Rooms;
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
internal sealed class PlaceObjectPacketHandler<T> : UserPacketHandler<T>
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

			user.Client.ScheduleTask(new PlaceFloorItemTask
			{
				DbContextFactory = this.dbContextFactory,

				FloorRoomItemStrategy = this.floorRoomItemStrategy,

				RoomUnit = roomUnit,

				Item = floorItem,

				Location = new Point2D(x, y),
				Direction = direction
			});
		}
		else if (item is IWallInventoryItem wallItem)
		{
			if (wallItem is IStickyNoteInventoryItem)
			{
				return;
			}

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

				user.Client.ScheduleTask(new PlaceWallItemTask
				{
					DbContextFactory = this.dbContextFactory,

					WallRoomItemStrategy = this.wallRoomItemStrategy,

					RoomUnit = roomUnit,

					Item = wallItem,

					Location = new Point2D(xLocation, yLocation),
					Position = new Point2D(xPosition, yPosition)
				});
			}
		}
	}

	[StructLayout(LayoutKind.Auto)]
	private readonly struct PlaceFloorItemTask : IClientTask, IRoomTask<Point3D?>
	{
		internal IDbContextFactory<SkylightContext> DbContextFactory { get; init; }

		internal IFloorRoomItemStrategy FloorRoomItemStrategy { get; init; }

		internal IUserRoomUnit RoomUnit { get; init; }

		internal IFloorInventoryItem Item { get; init; }

		internal Point2D Location { get; init; }
		internal int Direction { get; init; }

		public async Task ExecuteAsync(IClient client)
		{
			Point3D? position = await this.RoomUnit.Room.ScheduleTaskAsync<PlaceFloorItemTask, Point3D?>(this).ConfigureAwait(false);
			if (position is null)
			{
				return;
			}

			await using SkylightContext dbContext = await this.DbContextFactory.CreateDbContextAsync().ConfigureAwait(false);

			FloorItemEntity item = new()
			{
				Id = this.Item.Id,
				UserId = this.Item.Owner.Id
			};

			dbContext.FloorItems.Attach(item);

			item.RoomId = this.RoomUnit.Room.Info.Id;
			item.X = position.Value.X;
			item.Y = position.Value.Y;
			item.Z = position.Value.Z;

			await dbContext.SaveChangesAsync().ConfigureAwait(false);
		}

		public Point3D? Execute(IRoom room)
		{
			Point3D position = new(this.Location, room.ItemManager.GetPlacementHeight(this.Item.Furniture, this.Location));

			if (!this.RoomUnit.InRoom || !room.ItemManager.CanPlaceItem(this.Item.Furniture, position)
									  || !this.RoomUnit.User.Inventory.TryRemoveFloorItem(this.Item))
			{
				return null;
			}

			IFloorRoomItem item = this.FloorRoomItemStrategy.CreateFloorItem(room, this.Item, position, this.Direction);

			room.ItemManager.AddItem(item);

			return position;
		}
	}

	[StructLayout(LayoutKind.Auto)]
	private readonly struct PlaceWallItemTask : IClientTask, IRoomTask<(Point2D, Point2D)?>
	{
		internal IDbContextFactory<SkylightContext> DbContextFactory { get; init; }

		internal IWallRoomItemStrategy WallRoomItemStrategy { get; init; }

		internal IUserRoomUnit RoomUnit { get; init; }

		internal IWallInventoryItem Item { get; init; }

		internal Point2D Location { get; init; }
		internal Point2D Position { get; init; }

		public async Task ExecuteAsync(IClient client)
		{
			(Point2D Location, Point2D Position)? result = await this.RoomUnit.Room.ScheduleTaskAsync<PlaceWallItemTask, (Point2D, Point2D)?>(this).ConfigureAwait(false);
			if (result is null)
			{
				return;
			}

			await using SkylightContext dbContext = await this.DbContextFactory.CreateDbContextAsync().ConfigureAwait(false);

			WallItemEntity item = new()
			{
				Id = this.Item.Id,
				UserId = this.Item.Owner.Id
			};

			dbContext.WallItems.Attach(item);

			item.RoomId = this.RoomUnit.Room.Info.Id;
			item.LocationX = result.Value.Location.X;
			item.LocationY = result.Value.Location.Y;
			item.PositionX = result.Value.Position.X;
			item.PositionY = result.Value.Position.Y;

			await dbContext.SaveChangesAsync().ConfigureAwait(false);
		}

		public (Point2D, Point2D)? Execute(IRoom room)
		{
			if (!this.RoomUnit.InRoom || !room.ItemManager.CanPlaceItem(this.Item.Furniture, this.Location, this.Position)
									  || !this.RoomUnit.User.Inventory.TryRemoveWallItem(this.Item))
			{
				return null;
			}

			IWallRoomItem item = this.WallRoomItemStrategy.CreateWallItem(room, this.Item, this.Location, this.Position);

			room.ItemManager.AddItem(item);

			return (item.Location, item.Position);
		}
	}
}
