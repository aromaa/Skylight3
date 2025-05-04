using Net.Buffers;
using Net.Communication.Incoming.Consumer;
using Net.Communication.Manager;
using Net.Communication.Outgoing;
using Net.Sockets.Pipeline.Handler;
using Net.Sockets.Pipeline.Handler.Incoming;
using Net.Sockets.Pipeline.Handler.Outgoing;

namespace Skylight.Server.Net.Handlers;

internal abstract class PacketManagerHandler : IncomingBytesHandler, IOutgoingObjectHandler
{
	private protected abstract PacketManager<uint> PacketManager { get; }

	private protected void Read(IPipelineHandlerContext context, uint header, ref PacketReader reader)
	{
		if (this.PacketManager.TryGetConsumer(header, out IIncomingPacketConsumer? consumer))
		{
			consumer.Read(context, ref reader);

			if (reader.Readable)
			{
				Console.WriteLine($"Packet has stuff left: {header} ({reader.Remaining})");
			}
		}
		else
		{
			Console.WriteLine($"Unknown packet: {header}");
		}
	}

	public void Handle<T>(IPipelineHandlerContext context, ref PacketWriter writer, in T packet)
	{
		if (this.PacketManager.TryGetComposer<T>(out IOutgoingPacketComposer? composer, out uint header))
		{
			this.Handle(composer, ref writer, header, packet);
		}
		else
		{
			Console.WriteLine($"Missing composer: {typeof(T)}");
		}
	}

	private protected abstract void Handle<T>(IOutgoingPacketComposer composer, ref PacketWriter writer, uint header, in T packet);
}
