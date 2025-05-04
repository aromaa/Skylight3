using Net.Communication.Attributes;
using Skylight.API.Game.Users;
using Skylight.Protocol.Packets.Incoming.Help;
using Skylight.Protocol.Packets.Manager;

namespace Skylight.Server.Game.Communication.Help;

[PacketManagerRegister(typeof(IGamePacketManager))]
internal sealed class GetCfhStatusPacketHandler<T> : UserPacketHandler<T>
	where T : IGetCfhStatusIncomingPacket
{
	internal override void Handle(IUser user, in T packet)
	{
	}
}
