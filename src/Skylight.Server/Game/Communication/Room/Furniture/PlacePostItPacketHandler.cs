using System.Buffers;
using System.Buffers.Text;
using System.Runtime.InteropServices;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Net.Communication.Attributes;
using Skylight.API.Game.Clients;
using Skylight.API.Game.Inventory.Items;
using Skylight.API.Game.Inventory.Items.Wall;
using Skylight.API.Game.Rooms;
using Skylight.API.Game.Rooms.Items;
using Skylight.API.Game.Rooms.Items.Interactions;
using Skylight.API.Game.Rooms.Items.Wall;
using Skylight.API.Game.Rooms.Units;
using Skylight.API.Game.Users;
using Skylight.API.Numerics;
using Skylight.Domain.Items;
using Skylight.Infrastructure;
using Skylight.Protocol.Packets.Data.Room.Object.Data.Wall;
using Skylight.Protocol.Packets.Incoming.Room.Furniture;
using Skylight.Protocol.Packets.Manager;
using Skylight.Protocol.Packets.Outgoing.Inventory.Furni;
using Skylight.Protocol.Packets.Outgoing.Room.Furniture;

namespace Skylight.Server.Game.Communication.Room.Furniture;

[PacketManagerRegister(typeof(AbstractGamePacketManager))]
internal sealed class PlacePostItPacketHandler<T> : UserPacketHandler<T>
	where T : IPlacePostItIncomingPacket
{
	private readonly IDbContextFactory<SkylightContext> dbContextFactory;

	private readonly IWallRoomItemStrategy wallRoomItemStrategy;

	public PlacePostItPacketHandler(IDbContextFactory<SkylightContext> dbContextFactory, IWallRoomItemStrategy wallRoomItemStrategy)
	{
		this.dbContextFactory = dbContextFactory;

		this.wallRoomItemStrategy = wallRoomItemStrategy;
	}

	internal override void Handle(IUser user, in T packet)
	{
		if (user.RoomSession?.Unit is not { } roomUnit)
		{
			return;
		}

		if (!user.Inventory.TryGetFurnitureItem(packet.StripId, out IFurnitureInventoryItem? item) || item is not IStickyNoteInventoryItem postItItem)
		{
			return;
		}

		SequenceReader<byte> reader = new(packet.Location);

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

			user.Client.ScheduleTask(new PrePlacePostItTask
			{
				DbContextFactory = this.dbContextFactory,

				ItemStrategy = this.wallRoomItemStrategy,

				Unit = roomUnit,

				Item = postItItem,

				Location = new Point2D(xLocation, yLocation),
				Position = new Point2D(xPosition, yPosition)
			});
		}
	}

	[StructLayout(LayoutKind.Auto)]
	private readonly struct PrePlacePostItTask : IClientTask, IRoomTask<PrePlacePostItTask.StickiePlaceAction>
	{
		internal IDbContextFactory<SkylightContext> DbContextFactory { get; init; }

		internal IWallRoomItemStrategy ItemStrategy { get; init; }

		internal IUserRoomUnit Unit { get; init; }

		internal IStickyNoteInventoryItem Item { get; init; }

		internal Point2D Location { get; init; }
		internal Point2D Position { get; init; }

		public async Task ExecuteAsync(IClient client)
		{
			StickiePlaceAction canPlace = await this.Unit.Room.ScheduleTaskAsync<PrePlacePostItTask, StickiePlaceAction>(this).ConfigureAwait(false);
			switch (canPlace)
			{
				case StickiePlaceAction.HasRights:
				{
					IStickyNoteInventoryItem? item = await this.Item.TryConsumeAsync().ConfigureAwait(false);
					if (item is not null)
					{
						this.Unit.User.SendAsync(new PostItPlacedOutgoingPacket(this.Item.StripId, this.Item.Count));

						this.Unit.User.Inventory.TryAddWallItem(item);
					}
					else
					{
						item = this.Item;
					}

					(Point2D Location, Point2D Position, JsonDocument? ExtraData)? result = await this.Unit.Room.ScheduleTaskAsync<PlacePostItTask, (Point2D, Point2D, JsonDocument?)?>(new PlacePostItTask
					{
						ItemStrategy = this.ItemStrategy,

						Unit = this.Unit,

						Item = item,

						Location = this.Location,
						Position = this.Position
					}).ConfigureAwait(false);

					if (result is null)
					{
						return;
					}

					await using SkylightContext dbContext = await this.DbContextFactory.CreateDbContextAsync().ConfigureAwait(false);

					WallItemEntity itemEntity = new()
					{
						Id = item.Id,
						UserId = this.Item.Owner.Id
					};

					dbContext.WallItems.Attach(itemEntity);

					itemEntity.RoomId = this.Unit.Room.Info.Id;
					itemEntity.LocationX = result.Value.Location.X;
					itemEntity.LocationY = result.Value.Location.Y;
					itemEntity.PositionX = result.Value.Position.X;
					itemEntity.PositionY = result.Value.Position.Y;
					itemEntity.ExtraData = result.Value.ExtraData;

					await dbContext.SaveChangesAsync().ConfigureAwait(false);

					break;
				}

				case StickiePlaceAction.HasStickiePole:
					this.Unit.User.SendAsync(new RequestSpamWallPostItOutgoingPacket(this.Item.Id, new WallPosition(this.Location.X, this.Location.Y, this.Position.X, this.Position.Y)));
					break;
			}
		}

		public StickiePlaceAction Execute(IRoom room)
		{
			if (!this.Unit.InRoom || !room.ItemManager.CanPlaceItem(this.Item.Furniture, this.Location, this.Position, this.Unit.User))
			{
				return StickiePlaceAction.Blocked;
			}

			if (room.ItemManager.TryGetInteractionHandler(out IStickyNoteInteractionHandler? handler) && handler.HasStickyNotePole)
			{
				return StickiePlaceAction.HasStickiePole;
			}

			return StickiePlaceAction.HasRights;
		}

		internal enum StickiePlaceAction
		{
			Blocked,
			HasStickiePole,
			HasRights
		}
	}

	[StructLayout(LayoutKind.Auto)]
	private readonly struct PlacePostItTask : IRoomTask<(Point2D, Point2D, JsonDocument?)?>
	{
		internal IWallRoomItemStrategy ItemStrategy { get; init; }

		internal IUserRoomUnit Unit { get; init; }

		internal IStickyNoteInventoryItem Item { get; init; }

		internal Point2D Location { get; init; }
		internal Point2D Position { get; init; }

		public (Point2D, Point2D, JsonDocument?)? Execute(IRoom room)
		{
			if (!this.Unit.InRoom || !room.ItemManager.CanPlaceItem(this.Item.Furniture, this.Location, this.Position, this.Unit.User)
								  || !this.Unit.User.Inventory.TryRemoveWallItem(this.Item))
			{
				return null;
			}

			IStickyNoteRoomItem item = this.ItemStrategy.CreateWallItem(room, this.Item, this.Location, this.Position, this.Item.Furniture.DefaultColor);

			room.ItemManager.AddItem(item);

			return (item.Location, item.Position, item.AsExtraData());
		}
	}
}
