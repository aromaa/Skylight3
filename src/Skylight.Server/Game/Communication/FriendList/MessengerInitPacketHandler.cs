using Net.Communication.Attributes;
using Skylight.API.Game.Users;
using Skylight.Protocol.Packets.Data.FriendList;
using Skylight.Protocol.Packets.Incoming.FriendList;
using Skylight.Protocol.Packets.Manager;
using Skylight.Protocol.Packets.Outgoing.FriendList;

namespace Skylight.Server.Game.Communication.FriendList;

[PacketManagerRegister(typeof(AbstractGamePacketManager))]
internal sealed class MessengerInitPacketHandler<T> : UserPacketHandler<T>
	where T : IMessengerInitIncomingPacket
{
	internal override void Handle(IUser user, in T packet)
	{
		user.SendAsync(new MessengerInitOutgoingPacket(100, 200, 300, 600,
		[
			new FriendCategoryData(3, "Skylight")
		]));
	}
}
