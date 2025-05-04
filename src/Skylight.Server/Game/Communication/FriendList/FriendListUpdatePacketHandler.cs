using Net.Communication.Attributes;
using Skylight.API.Game.Users;
using Skylight.Protocol.Packets.Incoming.FriendList;
using Skylight.Protocol.Packets.Manager;

namespace Skylight.Server.Game.Communication.FriendList;

[PacketManagerRegister(typeof(IGamePacketManager))]
internal sealed class FriendListUpdatePacketHandler<T> : UserPacketHandler<T>
	where T : IFriendListUpdateIncomingPacket
{
	internal override void Handle(IUser user, in T packet)
	{
	}
}
