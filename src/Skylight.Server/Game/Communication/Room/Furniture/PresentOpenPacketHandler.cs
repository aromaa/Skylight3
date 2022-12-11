using System.Runtime.InteropServices;
using Net.Communication.Attributes;
using Skylight.API.Game.Clients;
using Skylight.API.Game.Furniture.Floor;
using Skylight.API.Game.Recycler.FurniMatic;
using Skylight.API.Game.Rooms;
using Skylight.API.Game.Rooms.Items.Floor;
using Skylight.API.Game.Rooms.Units;
using Skylight.API.Game.Users;
using Skylight.Protocol.Packets.Data.Room.Object;
using Skylight.Protocol.Packets.Incoming.Room.Furniture;
using Skylight.Protocol.Packets.Manager;
using Skylight.Protocol.Packets.Outgoing.Room.Furniture;

namespace Skylight.Server.Game.Communication.Room.Furniture;

[PacketManagerRegister(typeof(AbstractGamePacketManager))]
internal sealed class PresentOpenPacketHandler<T> : UserPacketHandler<T>
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

		user.Client.ScheduleTask(new OpenPresentTask
		{
			FurniMaticSnapshot = this.furniMaticManager.Current,

			Unit = roomUnit,

			ItemId = packet.ItemId
		});
	}

	[StructLayout(LayoutKind.Auto)]
	private readonly struct OpenPresentTask : IClientTask, IRoomTask, IRoomTask<IFloorRoomItem?>
	{
		internal IFurniMaticSnapshot FurniMaticSnapshot { get; init; }

		internal IUserRoomUnit Unit { get; init; }

		internal int ItemId { get; init; }

		public async Task ExecuteAsync(IClient client)
		{
			IFloorRoomItem? present = await this.Unit.Room.ScheduleTaskAsync<OpenPresentTask, IFloorRoomItem?>(this).ConfigureAwait(false);

			if (present is IFurniMaticGiftRoomItem roomItem)
			{
				IFurniMaticPrize? prize = await this.FurniMaticSnapshot.OpenGiftAsync(this.Unit.User, roomItem).ConfigureAwait(false);
				if (prize is null)
				{
					return;
				}

				client.SendAsync(new PresentOpenedOutgoingPacket(prize.Name, prize.Furnitures[0] is IFloorFurniture ? FurnitureType.Floor : FurnitureType.Wall, prize.Furnitures[0].Id, false, 0, FurnitureType.Floor, string.Empty));

				this.Unit.Room.ScheduleTask(this);
			}
		}

		public void Execute(IRoom room)
		{
			if (room.ItemManager.TryGetFloorItem(this.ItemId, out IFloorRoomItem? item))
			{
				room.ItemManager.RemoveItem(item);
			}
		}

		IFloorRoomItem? IRoomTask<IFloorRoomItem?>.Execute(IRoom room)
		{
			if (this.Unit.InRoom && room.ItemManager.TryGetFloorItem(this.ItemId, out IFloorRoomItem? item))
			{
				return item;
			}

			return null;
		}
	}
}
