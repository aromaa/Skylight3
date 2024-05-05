using Net.Communication.Attributes;
using Skylight.API.Game.Clients;
using Skylight.Protocol.Packets.Data.Registration;
using Skylight.Protocol.Packets.Incoming.Registration;
using Skylight.Protocol.Packets.Manager;
using Skylight.Protocol.Packets.Outgoing.Registration;

namespace Skylight.Server.Game.Communication.Registration;

[PacketManagerRegister(typeof(AbstractGamePacketManager))]
internal sealed class ApprovePasswordPacketHandler<T> : ClientPacketHandler<T>
	where T : IApprovePasswordIncomingPacket
{
	internal override void Handle(IClient client, in T packet)
	{
		client.SendAsync(new ApprovePasswordReplyOutgoingPacket(ApprovePasswordResult.Ok));
	}
}
