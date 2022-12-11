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
internal sealed class RecycleItemsPacketHandler<T> : UserPacketHandler<T>
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

		bool scheduled = user.Client.ScheduleTask(new RecycleTask
		{
			FurniMatic = snapshot,
			Items = items
		});

		if (!scheduled)
		{
			//1 completed
			//2 closed
			user.SendAsync(new RecyclerFinishedOutgoingPacket(2, 0));
		}
	}

	[StructLayout(LayoutKind.Auto)]
	private readonly struct RecycleTask : IClientTask
	{
		internal IFurniMaticSnapshot FurniMatic { get; init; }

		internal IFurnitureInventoryItem[] Items { get; init; }

		public async Task ExecuteAsync(IClient client)
		{
			IFurniMaticPrize? prize = await this.FurniMatic.RecycleAsync(client.User!, this.Items).ConfigureAwait(false);

			//1 completed
			//2 closed
			client.SendAsync(new RecyclerFinishedOutgoingPacket(prize is not null ? 1 : 2, 0));
		}
	}
}
