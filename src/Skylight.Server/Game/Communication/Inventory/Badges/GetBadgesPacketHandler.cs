using Net.Communication.Attributes;
using Skylight.API.Game.Inventory.Badges;
using Skylight.API.Game.Users;
using Skylight.Protocol.Packets.Incoming.Inventory.Badges;
using Skylight.Protocol.Packets.Manager;
using Skylight.Protocol.Packets.Outgoing.Inventory.Badges;

namespace Skylight.Server.Game.Communication.Inventory.Badges;

[PacketManagerRegister(typeof(IGamePacketManager))]
internal sealed class GetBadgesPacketHandler<T> : UserPacketHandler<T>
	where T : IGetBadgesIncomingPacket
{
	internal override void Handle(IUser user, in T packet)
	{
		IBadgeInventoryItem[][] fragments = user.Inventory.Badges.Chunk(2500).ToArray();

		int i = 0;

		foreach (IBadgeInventoryItem[] fragment in fragments)
		{
			user.SendAsync(new BadgesOutgoingPacket
			{
				TotalFragments = fragments.Length,
				FragmentId = i++,
				Fragment = fragment.Select(b => (b.Badge.Id, b.Badge.Code)).ToArray()
			});
		}

		if (i == 0)
		{
			user.SendAsync(new BadgesOutgoingPacket(1, 0, Array.Empty<(int, string)>()));
		}
	}
}
