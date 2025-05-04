using Net.Communication.Attributes;
using Skylight.API.Game.Users;
using Skylight.Protocol.Packets.Incoming.Tracking;
using Skylight.Protocol.Packets.Manager;

namespace Skylight.Server.Game.Communication.Tracking;

[PacketManagerRegister(typeof(IGamePacketManager))]
internal sealed class LatencyPingReportPacketHandler<T> : UserPacketHandler<T>
	where T : ILatencyPingReportIncomingPacket
{
	internal override void Handle(IUser user, in T packet)
	{
	}
}
