using Net.Communication.Manager;
using Skylight.Protocol.Packets.Manager;

namespace Skylight.Server.Net.Handlers;

internal sealed class HotSwapPacketHandler(Func<IGamePacketManager> packetManagerGetter) : AbstractPacketHeaderHandler
{
	private readonly Func<IGamePacketManager> packetManagerGetter = packetManagerGetter;

	private protected override PacketManager<uint> PacketManager => (PacketManager<uint>)this.packetManagerGetter();
}
