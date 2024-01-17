using System.Runtime.InteropServices;
using Net.Communication.Attributes;
using Skylight.API.Game.Clients;
using Skylight.API.Game.Inventory.Items;
using Skylight.API.Game.Recycler.FurniMatic;
using Skylight.API.Game.Users;
using Skylight.Protocol.Packets.Incoming.Recycler;
using Skylight.Protocol.Packets.Manager;
using Skylight.Protocol.Packets.Outgoing.Recycler;

namespace Skylight.Server.Game.Communication.Recycler;

[PacketManagerRegister(typeof(AbstractGamePacketManager))]
internal sealed partial class RecycleItemsPacketHandler<T> : UserPacketHandler<T>
	where T : IRecycleItemsIncomingPacket
{
	private readonly IFurniMaticManager furniMaticManager;

	public RecycleItemsPacketHandler(IFurniMaticManager furniMaticManager)
	{
		this.furniMaticManager = furniMaticManager;
	}

	internal override void Handle(IUser user, in T packet)
	{
		IFurniMaticSnapshot snapshot = this.furniMaticManager.Current;
		if (packet.StripIds.Count != snapshot.ItemsRequiredToRecycle)
		{
			user.SendAsync(new RecyclerFinishedOutgoingPacket(2, 0));
			return;
		}

		IFurnitureInventoryItem[] items = new IFurnitureInventoryItem[snapshot.ItemsRequiredToRecycle];
		for (int i = 0; i < items.Length; i++)
		{
			int stripId = packet.StripIds[i];

			if (!user.Inventory.TryGetFurnitureItem(stripId, out IFurnitureInventoryItem? item))
			{
				return;
			}

			items[i] = item;
		}

		bool scheduled = user.Client.ScheduleTask(async client =>
		{
			IFurniMaticPrize? prize = await this.furniMaticManager.RecycleAsync(client.User!, items).ConfigureAwait(false);

			//1 completed
			//2 closed
			client.SendAsync(new RecyclerFinishedOutgoingPacket(prize is not null ? 1 : 2, 0));
		});

		if (!scheduled)
		{
			//1 completed
			//2 closed
			user.SendAsync(new RecyclerFinishedOutgoingPacket(2, 0));
		}
	}
}
