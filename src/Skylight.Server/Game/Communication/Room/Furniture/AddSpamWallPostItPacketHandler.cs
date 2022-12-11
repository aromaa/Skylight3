using System.Buffers;
using System.Buffers.Text;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Net.Communication.Attributes;
using Skylight.API.Game.Clients;
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
using Skylight.Protocol.Packets.Incoming.Room.Furniture;
using Skylight.Protocol.Packets.Manager;
using Skylight.Protocol.Packets.Outgoing.Inventory.Furni;

namespace Skylight.Server.Game.Communication.Room.Furniture;

[PacketManagerRegister(typeof(AbstractGamePacketManager))]
internal sealed class AddSpamWallPostItPacketHandler<T> : UserPacketHandler<T>
	where T : IAddSpamWallPostItIncomingPacket
{
	private readonly IDbContextFactory<SkylightContext> dbContextFactory;

	private readonly IWallRoomItemStrategy wallRoomItemStrategy;

	public AddSpamWallPostItPacketHandler(IDbContextFactory<SkylightContext> dbContextFactory, IWallRoomItemStrategy wallRoomItemStrategy)
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

		if (!user.Inventory.TryGetWallItem(packet.Id, out IWallInventoryItem? item) || item is not IStickyNoteInventoryItem postItItem)
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

			if (!Utf8Parser.TryParse(packet.Color.IsSingleSegment ? packet.Color.FirstSpan : packet.Color.ToArray(), out int colorArgb, out _, 'X'))
			{
				return;
			}

			Color color = Color.FromArgb(colorArgb);

			if (!postItItem.Furniture.ValidColors.Contains(color))
			{
				return;
			}

			user.Client.ScheduleTask(new PrePlaceSpamWallPostItTask
			{
				DbContextFactory = this.dbContextFactory,

				ItemStrategy = this.wallRoomItemStrategy,

				Unit = roomUnit,

				Item = postItItem,

				Location = new Point2D(xLocation, yLocation),
				Position = new Point2D(xPosition, yPosition),

				Color = color,
				Text = Encoding.UTF8.GetString(packet.Text)
			});
		}
	}

	[StructLayout(LayoutKind.Auto)]
	private readonly struct PrePlaceSpamWallPostItTask : IClientTask, IRoomTask<bool>
	{
		internal IDbContextFactory<SkylightContext> DbContextFactory { get; init; }

		internal IWallRoomItemStrategy ItemStrategy { get; init; }

		internal IUserRoomUnit Unit { get; init; }

		internal IStickyNoteInventoryItem Item { get; init; }

		internal Point2D Location { get; init; }
		internal Point2D Position { get; init; }

		internal Color Color { get; init; }
		internal string Text { get; init; }

		public async Task ExecuteAsync(IClient client)
		{
			bool canPlace = await this.Unit.Room.ScheduleTaskAsync<PrePlaceSpamWallPostItTask, bool>(this).ConfigureAwait(false);
			if (!canPlace)
			{
				return;
			}

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

			(Point2D Location, Point2D Position, JsonDocument? ExtraData)? result = await this.Unit.Room.ScheduleTaskAsync<PlaceSpamWallPostItTask, (Point2D, Point2D, JsonDocument?)?>(new PlaceSpamWallPostItTask
			{
				ItemStrategy = this.ItemStrategy,

				Unit = this.Unit,

				Item = item,

				Location = this.Location,
				Position = this.Position,

				Color = this.Color,
				Text = this.Text
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
		}

		public bool Execute(IRoom room)
		{
			return this.Unit.InRoom && room.ItemManager.CanPlaceItem(this.Item.Furniture, this.Location, this.Position, this.Unit.User)
				&& room.ItemManager.TryGetInteractionHandler(out IStickyNoteInteractionHandler? handler) && handler.HasStickyNotePole;
		}
	}

	[StructLayout(LayoutKind.Auto)]
	private readonly struct PlaceSpamWallPostItTask : IRoomTask<(Point2D, Point2D, JsonDocument?)?>
	{
		internal IWallRoomItemStrategy ItemStrategy { get; init; }

		internal IUserRoomUnit Unit { get; init; }

		internal IStickyNoteInventoryItem Item { get; init; }

		internal Point2D Location { get; init; }
		internal Point2D Position { get; init; }

		internal Color Color { get; init; }
		internal string Text { get; init; }

		public (Point2D, Point2D, JsonDocument?)? Execute(IRoom room)
		{
			if (!this.Unit.InRoom || !room.ItemManager.CanPlaceItem(this.Item.Furniture, this.Location, this.Position, this.Unit.User)
								  || !this.Unit.User.Inventory.TryRemoveWallItem(this.Item))
			{
				return null;
			}

			IStickyNoteRoomItem item = this.ItemStrategy.CreateWallItem(room, this.Item, this.Location, this.Position, this.Color, this.Text);

			room.ItemManager.AddItem(item);

			return (item.Location, item.Position, item.AsExtraData());
		}
	}
}
