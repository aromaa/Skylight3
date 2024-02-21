using Skylight.Protocol.Packets.Manager;

namespace Skylight.Server.Net.Handlers;

internal sealed class PacketHeaderHandler(AbstractGamePacketManager packetManager) : AbstractPacketHeaderHandler
{
	private protected override AbstractGamePacketManager PacketManager { get; } = packetManager;
}
