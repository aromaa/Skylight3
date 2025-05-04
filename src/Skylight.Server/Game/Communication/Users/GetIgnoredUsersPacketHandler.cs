using Net.Communication.Attributes;
using Skylight.API.Game.Users;
using Skylight.Protocol.Packets.Incoming.Users;
using Skylight.Protocol.Packets.Manager;
using Skylight.Protocol.Packets.Outgoing.Users;

namespace Skylight.Server.Game.Communication.Users;

[PacketManagerRegister(typeof(IGamePacketManager))]
internal sealed class GetIgnoredUsersPacketHandler<T> : UserPacketHandler<T>
	where T : IGetIgnoredUsersIncomingPacket
{
	internal override void Handle(IUser user, in T packet)
	{
		user.SendAsync(new IgnoredUsersOutgoingPacket(Array.Empty<string>()));
	}
}
