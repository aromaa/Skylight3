using Skylight.Protocol.Packets.Manager;

namespace Skylight.Server.Net.Handlers;

internal sealed class HotSwapPacketHandler : AbstractPacketHeaderHandler
{
	private readonly Func<AbstractGamePacketManager> packetManagerGetter;

	public HotSwapPacketHandler(Func<AbstractGamePacketManager> packetManagerGetter)
	{
		this.packetManagerGetter = packetManagerGetter;
	}

	private protected override AbstractGamePacketManager PacketManager => this.packetManagerGetter();
}
