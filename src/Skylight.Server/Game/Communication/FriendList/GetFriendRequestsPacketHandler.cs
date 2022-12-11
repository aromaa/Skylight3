using Net.Communication.Attributes;
using Skylight.API.Game.Users;
using Skylight.Protocol.Packets.Data.FriendList;
using Skylight.Protocol.Packets.Incoming.FriendList;
using Skylight.Protocol.Packets.Manager;
using Skylight.Protocol.Packets.Outgoing.FriendList;

namespace Skylight.Server.Game.Communication.FriendList;

[PacketManagerRegister(typeof(AbstractGamePacketManager))]
internal sealed class GetFriendRequestsPacketHandler<T> : UserPacketHandler<T>
	where T : IGetFriendRequestsIncomingPacket
{
	internal override void Handle(IUser user, in T packet)
	{
		user.SendAsync(new FriendRequestsOutgoingPacket(0, Array.Empty<FriendRequestData>()));
	}
}
