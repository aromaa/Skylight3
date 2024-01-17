using System.Buffers;
using System.Buffers.Text;
using System.Drawing;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Net.Communication.Attributes;
using Skylight.API.Game.Inventory.Items.Wall;
using Skylight.API.Game.Rooms.Items;
using Skylight.API.Game.Rooms.Items.Interactions;
using Skylight.API.Game.Rooms.Items.Wall;
using Skylight.API.Game.Users;
using Skylight.API.Numerics;
using Skylight.Domain.Items;
using Skylight.Infrastructure;
using Skylight.Protocol.Packets.Incoming.Room.Furniture;
using Skylight.Protocol.Packets.Manager;
using Skylight.Protocol.Packets.Outgoing.Inventory.Furni;

namespace Skylight.Server.Game.Communication.Room.Furniture;

[PacketManagerRegister(typeof(AbstractGamePacketManager))]
internal sealed partial class AddSpamWallPostItPacketHandler<T> : UserPacketHandler<T>
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

			Point2D location = new(xLocation, yLocation);
			Point2D position = new(xPosition, yPosition);

			string text = Encoding.UTF8.GetString(packet.Text);

			user.Client.ScheduleTask(async _ =>
			{
				bool canPlace = await roomUnit.Room.ScheduleTask(room =>
				{
					return roomUnit.InRoom && room.ItemManager.CanPlaceItem(postItItem.Furniture, location, position, roomUnit.User)
											 && room.ItemManager.TryGetInteractionHandler(out IStickyNoteInteractionHandler? handler) && handler.HasStickyNotePole;
				}).ConfigureAwait(false);

				if (!canPlace)
				{
					return;
				}

				IStickyNoteInventoryItem? item = await postItItem.TryConsumeAsync().ConfigureAwait(false);
				if (item is not null)
				{
					roomUnit.User.SendAsync(new PostItPlacedOutgoingPacket(postItItem.StripId, postItItem.Count));

					roomUnit.User.Inventory.TryAddWallItem(item);
				}
				else
				{
					item = postItItem;
				}

				WallItemEntity? result = await roomUnit.Room.ScheduleTask(room =>
				{
					if (!roomUnit.InRoom || !room.ItemManager.CanPlaceItem(postItItem.Furniture, location, position, roomUnit.User)
										   || !roomUnit.User.Inventory.TryRemoveWallItem(postItItem))
					{
						return null;
					}

					IStickyNoteRoomItem item = this.wallRoomItemStrategy.CreateWallItem(room, postItItem, location, position, color, text);

					room.ItemManager.AddItem(item);

					return new WallItemEntity
					{
						Id = item.Id,
						UserId = postItItem.Owner.Id,

						LocationX = item.Location.X,
						LocationY = item.Location.Y,
						PositionX = item.Position.X,
						PositionY = item.Position.Y,
						ExtraData = item.AsExtraData()
					};
				}).ConfigureAwait(false);

				if (result is null)
				{
					return;
				}

				await using SkylightContext dbContext = await this.dbContextFactory.CreateDbContextAsync().ConfigureAwait(false);

				dbContext.WallItems.Update(result);

				await dbContext.SaveChangesAsync().ConfigureAwait(false);
			});
		}
	}
}
