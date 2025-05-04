using Net.Communication.Attributes;
using Skylight.API.Game.Clients;
using Skylight.Protocol.Packets.Incoming.Registration;
using Skylight.Protocol.Packets.Manager;
using Skylight.Protocol.Packets.Outgoing.Registration;

namespace Skylight.Server.Game.Communication.Registration;

[PacketManagerRegister(typeof(IGamePacketManager))]
internal sealed class ServerDatePacketHandler<T> : ClientPacketHandler<T>
	where T : IGetServerDateIncomingPacket
{
	internal override void Handle(IClient client, in T packet)
	{
		client.SendAsync(new ServerDateOutgoingPacket(DateTime.Now));
	}
}
