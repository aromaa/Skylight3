using Net.Communication.Attributes;
using Skylight.API.Game.Users;
using Skylight.Protocol.Packets.Incoming.Tracking;
using Skylight.Protocol.Packets.Manager;
using Skylight.Protocol.Packets.Outgoing.Tracking;

namespace Skylight.Server.Game.Communication.Tracking;

[PacketManagerRegister(typeof(IGamePacketManager))]
internal sealed class LatencyPingRequestPacketHandler<T> : UserPacketHandler<T>
	where T : ILatencyPingRequestIncomingPacket
{
	internal override void Handle(IUser user, in T packet)
	{
		user.SendAsync(new LatencyPingResponseOutgoingPacket(packet.RequestId));
	}
}
