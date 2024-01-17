using Net.Communication.Attributes;
using Skylight.API.Game.Furniture.Floor;
using Skylight.API.Game.Recycler.FurniMatic;
using Skylight.API.Game.Rooms.Items.Floor;
using Skylight.API.Game.Users;
using Skylight.Protocol.Packets.Data.Room.Object;
using Skylight.Protocol.Packets.Incoming.Room.Furniture;
using Skylight.Protocol.Packets.Manager;
using Skylight.Protocol.Packets.Outgoing.Room.Furniture;

namespace Skylight.Server.Game.Communication.Room.Furniture;

[PacketManagerRegister(typeof(AbstractGamePacketManager))]
internal sealed partial class PresentOpenPacketHandler<T> : UserPacketHandler<T>
	where T : IPresentOpenIncomingPacket
{
	private readonly IFurniMaticManager furniMaticManager;

	public PresentOpenPacketHandler(IFurniMaticManager furniMaticManager)
	{
		this.furniMaticManager = furniMaticManager;
	}

	internal override void Handle(IUser user, in T packet)
	{
		if (user.RoomSession?.Unit is not { } roomUnit)
		{
			return;
		}

		int itemId = packet.ItemId;

		user.Client.ScheduleTask(async client =>
		{
			IFloorRoomItem? present = await roomUnit.Room.ScheduleTask(room =>
			{
				if (roomUnit.InRoom && room.ItemManager.TryGetFloorItem(itemId, out IFloorRoomItem? item))
				{
					return item;
				}

				return null;
			}).ConfigureAwait(false);

			if (present is IFurniMaticGiftRoomItem roomItem)
			{
				IFurniMaticPrize? prize = await this.furniMaticManager.OpenGiftAsync(roomUnit.User, roomItem).ConfigureAwait(false);
				if (prize is null)
				{
					return;
				}

				client.SendAsync(new PresentOpenedOutgoingPacket(prize.Name, prize.Furnitures[0] is IFloorFurniture ? FurnitureType.Floor : FurnitureType.Wall, prize.Furnitures[0].Id, false, 0, FurnitureType.Floor, string.Empty));

				roomUnit.Room.PostTask(room =>
				{
					if (room.ItemManager.TryGetFloorItem(itemId, out IFloorRoomItem? item))
					{
						room.ItemManager.RemoveItem(item);
					}
				});
			}
		});
	}
}
