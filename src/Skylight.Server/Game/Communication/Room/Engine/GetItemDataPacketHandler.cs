using Net.Communication.Attributes;
using Skylight.API.Game.Users;
using Skylight.Protocol.Packets.Incoming.Room.Engine;
using Skylight.Protocol.Packets.Manager;

namespace Skylight.Server.Game.Communication.Room.Engine;

[PacketManagerRegister(typeof(IGamePacketManager))]
internal sealed class GetItemDataPacketHandler<T> : UserPacketHandler<T>
	where T : IGetItemDataIncomingPacket
{
	internal override void Handle(IUser user, in T packet)
	{
	}
}
