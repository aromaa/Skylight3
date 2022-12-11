using Net.Communication.Attributes;
using Skylight.API.Game.Users;
using Skylight.Protocol.Packets.Incoming.Preferences;
using Skylight.Protocol.Packets.Manager;

namespace Skylight.Server.Game.Communication.Preferences;

[PacketManagerRegister(typeof(AbstractGamePacketManager))]
internal sealed class SetNewNavigatorWindowPreferencesPacketHandler<T> : UserPacketHandler<T>
	where T : ISetNewNavigatorWindowPreferencesIncomingPacket
{
	internal override void Handle(IUser user, in T packet)
	{
	}
}
