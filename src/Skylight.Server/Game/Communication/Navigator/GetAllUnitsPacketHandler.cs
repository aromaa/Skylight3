using Net.Communication.Attributes;
using Skylight.API.Game.Users;
using Skylight.Protocol.Packets.Data.Navigator;
using Skylight.Protocol.Packets.Incoming.Navigator;
using Skylight.Protocol.Packets.Manager;
using Skylight.Protocol.Packets.Outgoing.Navigator;

namespace Skylight.Server.Game.Communication.Navigator;

[PacketManagerRegister(typeof(IGamePacketManager))]
internal sealed class GetAllUnitsPacketHandler<T> : UserPacketHandler<T>
	where T : IGetAllUnitsIncomingPacket
{
	internal override void Handle(IUser user, in T packet)
	{
		user.SendAsync(new PrivateUnitsOutgoingPacket([new PrivateUnitData("127.0.0.1", 20300)]));
	}
}
