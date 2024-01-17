using System.Buffers;
using System.Buffers.Text;
using Microsoft.EntityFrameworkCore;
using Net.Communication.Attributes;
using Skylight.API.Game.Inventory.Items;
using Skylight.API.Game.Inventory.Items.Wall;
using Skylight.API.Game.Rooms.Items;
using Skylight.API.Game.Rooms.Items.Interactions;
using Skylight.API.Game.Rooms.Items.Wall;
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
internal sealed partial class PlacePostItPacketHandler<T> : UserPacketHandler<T>
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

			Point2D location = new(xLocation, yLocation);
			Point2D position = new(xPosition, yPosition);

			user.Client.ScheduleTask(async _ =>
			{
				WallItemEntity? itemEntity = await roomUnit.Room.ScheduleTaskAsync(async room =>
				{
					if (!roomUnit.InRoom || !room.ItemManager.CanPlaceItem(postItItem.Furniture, location, position, roomUnit.User))
					{
						return default;
					}

					if (room.ItemManager.TryGetInteractionHandler(out IStickyNoteInteractionHandler? handler) && handler.HasStickyNotePole)
					{
						roomUnit.User.SendAsync(new RequestSpamWallPostItOutgoingPacket(postItItem.Id, new WallPosition(location.X, location.Y, position.X, position.Y)));

						return default;
					}

					IStickyNoteInventoryItem? inventoryItem = await postItItem.TryConsumeAsync().ConfigureAwait(true);
					if (inventoryItem is not null)
					{
						roomUnit.User.SendAsync(new PostItPlacedOutgoingPacket(postItItem.StripId, postItItem.Count));

						roomUnit.User.Inventory.TryAddWallItem(inventoryItem);
					}
					else
					{
						inventoryItem = postItItem;
					}

					if (!roomUnit.InRoom || !room.ItemManager.CanPlaceItem(inventoryItem.Furniture, location, position, roomUnit.User)
										  || !roomUnit.User.Inventory.TryRemoveWallItem(inventoryItem))
					{
						return default;
					}

					IStickyNoteRoomItem wallItem = this.wallRoomItemStrategy.CreateWallItem(room, inventoryItem, location, position, inventoryItem.Furniture.DefaultColor);

					room.ItemManager.AddItem(wallItem);

					return new WallItemEntity
					{
						Id = wallItem.Id,
						UserId = postItItem.Owner.Id,
						RoomId = roomUnit.Room.Info.Id,
						LocationX = wallItem.Location.X,
						LocationY = wallItem.Location.Y,
						PositionX = wallItem.Position.X,
						PositionY = wallItem.Position.Y,
					};
				}).ConfigureAwait(false);

				if (itemEntity is not null)
				{
					await using SkylightContext dbContext = await this.dbContextFactory.CreateDbContextAsync().ConfigureAwait(false);

					//var e = dbContext.WallItems.Update(itemEntity);

					var e = dbContext.WallItems.Entry(itemEntity);

					e.CurrentValues.SetValues(itemEntity);

					dbContext.WallItems.Update(itemEntity);

					await dbContext.SaveChangesAsync().ConfigureAwait(false);
				}
			});
		}
	}
}
