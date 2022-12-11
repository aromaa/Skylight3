using Net.Communication.Attributes;
using Skylight.API.Game.Furniture.Floor;
using Skylight.API.Game.Inventory.Items;
using Skylight.API.Game.Users;
using Skylight.Protocol.Packets.Data.Inventory.Furni;
using Skylight.Protocol.Packets.Data.Room.Object;
using Skylight.Protocol.Packets.Incoming.Inventory.Furni;
using Skylight.Protocol.Packets.Manager;
using Skylight.Protocol.Packets.Outgoing.Inventory.Furni;
using Skylight.Server.Extensions;

namespace Skylight.Server.Game.Communication.Inventory.Furni;

[PacketManagerRegister(typeof(AbstractGamePacketManager))]
internal sealed class RequestFurniInventoryPacketHandler<T> : UserPacketHandler<T>
	where T : IRequestFurniInventoryIncomingPacket
{
	internal override void Handle(IUser user, in T packet)
	{
		List<IFurnitureInventoryItem[]> fragments = user.Inventory.FloorItems
			.Cast<IFurnitureInventoryItem>()
			.Concat(user.Inventory.WallItems)
			.Chunk(2500)
			.ToList();

		int i = 0;

		foreach (IFurnitureInventoryItem[]? fragment in fragments)
		{
			List<InventoryItemData> list = fragment.Select(static i => i.Furniture is IFloorFurniture
				? new InventoryItemData(i.StripId, i.Id, i.Furniture.Id, FurnitureType.Floor, i.GetItemCategory(), i.GetItemData())
				: new InventoryItemData(i.StripId, i.Id, i.Furniture.Id, FurnitureType.Wall, i.GetItemCategory(), i.GetItemData())).ToList();

			user.SendAsync(new FurniListOutgoingPacket
			{
				TotalFragments = fragments.Count,
				FragmentId = i++,
				Fragment = list
			});
		}
	}
}
