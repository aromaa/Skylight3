using Net.Communication.Incoming.Handler;
using Net.Sockets.Pipeline.Handler;
using Skylight.API.Game.Clients;
using Skylight.Protocol.Packets.Incoming;
using Skylight.Server.Game.Clients;
using Skylight.Server.Net;

namespace Skylight.Server.Game.Communication;

internal abstract class ClientPacketHandler<T> : IIncomingPacketHandler<T>
	where T : IGameIncomingPacket
{
	public void Handle(IPipelineHandlerContext context, in T packet)
	{
		if (context.Socket.Metadata.TryGetValue(NetworkManager.GameClientMetadataKey, out Client gameClient))
		{
			this.Handle(gameClient, packet);
		}
	}

	internal abstract void Handle(IClient client, in T packet);
}
