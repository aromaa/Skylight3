using Net.Communication.Attributes;
using Skylight.API.Game.Users;
using Skylight.Protocol.Packets.Incoming.Room.Engine;
using Skylight.Protocol.Packets.Manager;
using Skylight.Protocol.Packets.Outgoing.Room.Engine;

namespace Skylight.Server.Game.Communication.Room.Engine;

[PacketManagerRegister(typeof(AbstractGamePacketManager))]
internal sealed class GetFurnitureAliasesPacketHandler<T> : UserPacketHandler<T>
	where T : IGetFurnitureAliasesIncomingPacket
{
	internal override void Handle(IUser user, in T packet)
	{
		user.SendAsync(new FurnitureAliasesOutgoingPacket(Array.Empty<(string, string)>()));
	}
}
