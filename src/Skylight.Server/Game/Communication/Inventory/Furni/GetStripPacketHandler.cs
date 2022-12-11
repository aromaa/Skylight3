using Net.Communication.Attributes;
using Skylight.API.Game.Users;
using Skylight.Protocol.Packets.Incoming.Inventory.Furni;
using Skylight.Protocol.Packets.Manager;

namespace Skylight.Server.Game.Communication.Inventory.Furni;

[PacketManagerRegister(typeof(AbstractGamePacketManager))]
internal sealed class GetStripPacketHandler<T> : UserPacketHandler<T>
	where T : IGetStripIncomingPacket
{
	internal override void Handle(IUser user, in T packet)
	{
	}
}
