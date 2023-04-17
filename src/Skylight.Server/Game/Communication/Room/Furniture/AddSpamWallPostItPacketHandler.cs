using System.Buffers;
using System.Buffers.Text;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Net.Communication.Attributes;
using Skylight.API.Game.Clients;
using Skylight.API.Game.Inventory.Items.Wall;
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
	private readonly struct PrePlaceSpamWallPostItTask : IClientTask
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
			bool canPlace = await this.Unit.Room.ScheduleTaskAsync(static (room, state) =>
			{
				return state.Unit.InRoom && room.ItemManager.CanPlaceItem(state.Item.Furniture, state.Location, state.Position, state.Unit.User)
										 && room.ItemManager.TryGetInteractionHandler(out IStickyNoteInteractionHandler? handler) && handler.HasStickyNotePole;
			}, (this.Unit, this.Item, this.Location, this.Position)).ConfigureAwait(false);

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

			WallItemEntity? result = await this.Unit.Room.ScheduleTaskAsync(static (room, state) =>
			{
				if (!state.Unit.InRoom || !room.ItemManager.CanPlaceItem(state.Item.Furniture, state.Location, state.Position, state.Unit.User)
									   || !state.Unit.User.Inventory.TryRemoveWallItem(state.Item))
				{
					return null;
				}

				IStickyNoteRoomItem item = state.ItemStrategy.CreateWallItem(room, state.Item, state.Location, state.Position, state.Color, state.Text);

				room.ItemManager.AddItem(item);

				return new WallItemEntity
				{
					Id = item.Id,
					UserId = state.Item.Owner.Id,

					LocationX = item.Location.X,
					LocationY = item.Location.Y,
					PositionX = item.Position.X,
					PositionY = item.Position.Y,
					ExtraData = item.AsExtraData()
				};
			}, (this.ItemStrategy, this.Unit, Item: item, this.Location, this.Position, this.Color, this.Text)).ConfigureAwait(false);

			if (result is null)
			{
				return;
			}

			await using SkylightContext dbContext = await this.DbContextFactory.CreateDbContextAsync().ConfigureAwait(false);

			dbContext.WallItems.Update(result);

			await dbContext.SaveChangesAsync().ConfigureAwait(false);
		}
	}
}
