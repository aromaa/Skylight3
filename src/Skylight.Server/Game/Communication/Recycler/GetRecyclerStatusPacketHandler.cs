using Net.Communication.Attributes;
using Skylight.API.Game.Users;
using Skylight.Protocol.Packets.Incoming.Recycler;
using Skylight.Protocol.Packets.Manager;
using Skylight.Protocol.Packets.Outgoing.Recycler;

namespace Skylight.Server.Game.Communication.Recycler;

[PacketManagerRegister(typeof(IGamePacketManager))]
internal sealed class GetRecyclerStatusPacketHandler<T> : UserPacketHandler<T>
	where T : IGetRecyclerStatusIncomingPacket
{
	internal override void Handle(IUser user, in T packet)
	{
		//1 ready
		//2 closed
		//3 timeout
		user.SendAsync(new RecyclerStatusOutgoingPacket(1, -1));
	}
}
