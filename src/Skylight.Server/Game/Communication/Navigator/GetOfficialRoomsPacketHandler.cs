using Net.Communication.Attributes;
using Skylight.API.Game.Users;
using Skylight.Protocol.Packets.Incoming.Navigator;
using Skylight.Protocol.Packets.Manager;
using Skylight.Protocol.Packets.Outgoing.Navigator;

namespace Skylight.Server.Game.Communication.Navigator;

[PacketManagerRegister(typeof(AbstractGamePacketManager))]
internal sealed class GetOfficialRoomsPacketHandler<T> : UserPacketHandler<T>
	where T : IGetOfficialRoomsIncomingPacket
{
	internal override void Handle(IUser user, in T packet)
	{
		user.SendAsync(new OfficialRoomsOutgoingPacket(packet.NodeMask, packet.NodeId));
	}
}
