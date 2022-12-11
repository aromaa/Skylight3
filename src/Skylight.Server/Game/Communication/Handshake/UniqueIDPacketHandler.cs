using System.Buffers;
using Net.Communication.Attributes;
using Skylight.API.Game.Clients;
using Skylight.Protocol.Packets.Incoming.Handshake;
using Skylight.Protocol.Packets.Manager;
using Skylight.Protocol.Packets.Outgoing.Handshake;

namespace Skylight.Server.Game.Communication.Handshake;

[PacketManagerRegister(typeof(AbstractGamePacketManager))]
internal sealed class UniqueIDPacketHandler<T> : ClientPacketHandler<T>
	where T : IUniqueIDIncomingPacket
{
	internal override void Handle(IClient client, in T packet)
	{
		bool isCorruptedPacket = packet.FlashVersion.IsEmpty;

		ReadOnlySequence<byte> machineId = isCorruptedPacket ? default : packet.MachineId;
		ReadOnlySequence<byte> fingerprint = isCorruptedPacket ? packet.MachineId : packet.Fingerprint;
		ReadOnlySequence<byte> flashVersion = isCorruptedPacket ? packet.Fingerprint : packet.FlashVersion;

		if (machineId.IsEmpty)
		{
			client.SendAsync(new UniqueMachineIDOutgoingPacket("SKYLIGHT"));
		}
	}
}
