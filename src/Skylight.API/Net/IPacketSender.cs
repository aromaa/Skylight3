using Skylight.Protocol.Packets.Outgoing;

namespace Skylight.API.Net;

public interface IPacketSender
{
	public void SendAsync<T>(in T packet)
		where T : IGameOutgoingPacket;
}
