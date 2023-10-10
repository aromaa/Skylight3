using Skylight.Protocol.Packets.Manager;

namespace Skylight.Server.Net.Handlers;

internal sealed class PacketHeaderHandler : AbstractPacketHeaderHandler
{
	private protected override AbstractGamePacketManager PacketManager { get; }

	public PacketHeaderHandler(AbstractGamePacketManager packetManager)
	{
		this.PacketManager = packetManager;
	}
}
