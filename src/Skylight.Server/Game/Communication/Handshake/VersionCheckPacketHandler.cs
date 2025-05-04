using Net.Communication.Attributes;
using Skylight.API.Game.Clients;
using Skylight.Protocol.Packets.Incoming.Handshake;
using Skylight.Protocol.Packets.Manager;

namespace Skylight.Server.Game.Communication.Handshake;

[PacketManagerRegister(typeof(IGamePacketManager))]
internal sealed class VersionCheckPacketHandler<T> : ClientPacketHandler<T>
	where T : IVersionCheckIncomingPacket
{
	internal override void Handle(IClient client, in T packet)
	{
	}
}
