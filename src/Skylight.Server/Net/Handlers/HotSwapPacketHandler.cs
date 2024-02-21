using Skylight.Protocol.Packets.Manager;

namespace Skylight.Server.Net.Handlers;

internal sealed class HotSwapPacketHandler(Func<AbstractGamePacketManager> packetManagerGetter) : AbstractPacketHeaderHandler
{
	private readonly Func<AbstractGamePacketManager> packetManagerGetter = packetManagerGetter;

	private protected override AbstractGamePacketManager PacketManager => this.packetManagerGetter();
}
