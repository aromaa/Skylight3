using System.Text;
using Net.Communication.Attributes;
using Skylight.API.Game.Users;
using Skylight.Protocol.Packets.Incoming.Competition;
using Skylight.Protocol.Packets.Manager;
using Skylight.Protocol.Packets.Outgoing.Competition;

namespace Skylight.Server.Game.Communication.Competition;

[PacketManagerRegister(typeof(AbstractGamePacketManager))]
internal sealed class GetCurrentTimingCodePacketHandler<T> : UserPacketHandler<T>
	where T : IGetCurrentTimingCodeIncomingPacket
{
	internal override void Handle(IUser user, in T packet)
	{
		//TODO: Parsing
		user.SendAsync(new CurrentTimingCodeOutgoingPacket(Encoding.UTF8.GetString(packet.SchedulingCode), Encoding.UTF8.GetString(packet.SchedulingCode)));
	}
}
