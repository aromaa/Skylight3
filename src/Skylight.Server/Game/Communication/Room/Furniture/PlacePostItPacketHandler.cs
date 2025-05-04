using System.Buffers;
using System.Buffers.Text;
using Microsoft.EntityFrameworkCore;
using Net.Communication.Attributes;
using Skylight.API.Game.Inventory.Items;
using Skylight.API.Game.Inventory.Items.Wall;
using Skylight.API.Game.Rooms.Items.Interactions;
using Skylight.API.Game.Rooms.Items.Wall;
using Skylight.API.Game.Rooms.Private;
using Skylight.API.Game.Users;
using Skylight.API.Numerics;
using Skylight.Infrastructure;
using Skylight.Protocol.Packets.Data.Room.Object.Data.Wall;
using Skylight.Protocol.Packets.Incoming.Room.Furniture;
using Skylight.Protocol.Packets.Manager;
using Skylight.Protocol.Packets.Outgoing.Inventory.Furni;
using Skylight.Protocol.Packets.Outgoing.Room.Furniture;
using Skylight.Server.Extensions;

namespace Skylight.Server.Game.Communication.Room.Furniture;

[PacketManagerRegister(typeof(IGamePacketManager))]
internal sealed partial class PlacePostItPacketHandler<T>(IDbContextFactory<SkylightContext> dbContextFactory, IWallRoomItemStrategy wallRoomItemStrategy)
	: UserPacketHandler<T>
	where T : IPlacePostItIncomingPacket
{
	private readonly IDbContextFactory<SkylightContext> dbContextFactory = dbContextFactory;

	private readonly IWallRoomItemStrategy wallRoomItemStrategy = wallRoomItemStrategy;

	internal override void Handle(IUser user, in T packet)
	{
		if (user.RoomSession?.Unit is not { Room: IPrivateRoom privateRoom } roomUnit)
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
				bool canPlace = roomUnit.Room.ScheduleTask(_ =>
				{
					if (!roomUnit.InRoom || !privateRoom.ItemManager.CanPlaceItem(postItItem.Furniture, location, position, direction, roomUnit.User))
					{
						return false;
					}

					if (privateRoom.ItemManager.TryGetInteractionHandler(out IStickyNoteInteractionHandler? handler) && handler.HasStickyNotePole)
					{
						roomUnit.User.SendAsync(new RequestSpamWallPostItOutgoingPacket(postItItem.Id, new WallPosition(location.X, location.Y, position.X, position.Y)));

						return false;
					}

					return true;
				}).TryGetOrSuppressThrowing(out bool canPlaceAwait, out ValueTaskExtensions.Awaiter<bool> canPlaceAwaiter) ? canPlaceAwait : await canPlaceAwaiter;

				if (!canPlace)
				{
					return;
				}

				IStickyNoteInventoryItem? inventoryItem = await postItItem.TryConsumeAsync(privateRoom.Info.Id).ConfigureAwait(false);
				if (inventoryItem is null)
				{
					return;
				}
				else if (inventoryItem == postItItem)
				{
					if (!user.Inventory.TryRemoveWallItem(inventoryItem))
					{
						return;
					}
				}

				roomUnit.User.SendAsync(new PostItPlacedOutgoingPacket(postItItem.StripId, postItItem.Count));

				bool placed = roomUnit.Room.ScheduleTask(_ =>
				{
					if (!roomUnit.InRoom || !privateRoom.ItemManager.CanPlaceItem(inventoryItem.Furniture, location, position, direction, roomUnit.User))
					{
						return false;
					}

					privateRoom.ItemManager.AddItem(this.wallRoomItemStrategy.CreateWallItem(inventoryItem, privateRoom, location, position));

					return true;
				}).TryGetOrSuppressThrowing(out bool placeAwait, out ValueTaskExtensions.Awaiter<bool> placeAwaiter) ? placeAwait : await placeAwaiter;

				if (!placed)
				{
					await using (SkylightContext dbContext = await this.dbContextFactory.CreateDbContextAsync().ConfigureAwait(false))
					{
						int itemId = inventoryItem.Id;
						int userId = roomUnit.User.Profile.Id;
						int roomId = privateRoom.Info.Id;

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

					roomUnit.User.Inventory.TryAddWallItem(inventoryItem);
				}
			});
		}
	}
}
