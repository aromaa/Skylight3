using Net.Buffers;
using Net.Communication.Incoming.Consumer;
using Net.Communication.Outgoing;
using Net.Sockets.Pipeline.Handler;
using Net.Sockets.Pipeline.Handler.Incoming;
using Net.Sockets.Pipeline.Handler.Outgoing;
using Skylight.Protocol.Packets.Manager;

namespace Skylight.Server.Net.Handlers;

internal sealed class PacketHeaderHandler : IncomingBytesHandler, IOutgoingObjectHandler
{
	private readonly AbstractGamePacketManager packetManager;

	private uint currentPacketLength;

	internal PacketHeaderHandler(AbstractGamePacketManager packetManager)
	{
		this.packetManager = packetManager;
	}

	protected override void Decode(IPipelineHandlerContext context, ref PacketReader reader)
	{
		//We haven't read the next packet length, wait for it
		if (this.currentPacketLength == 0 && !reader.TryReadUInt32(out this.currentPacketLength))
		{
			return;
		}

		if (reader.Remaining < this.currentPacketLength)
		{
			return;
		}

		PacketReader readerSliced = reader.Slice(this.currentPacketLength);

		this.Read(context, ref readerSliced);

		this.currentPacketLength = 0;
	}

	public void Read(IPipelineHandlerContext context, ref PacketReader reader)
	{
		ushort header = reader.ReadUInt16();

		if (this.packetManager.TryGetConsumer(header, out IIncomingPacketConsumer? consumer))
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
		if (this.packetManager.TryGetComposer<T>(out IOutgoingPacketComposer? composer, out uint header))
		{
			PacketWriter slice = writer.ReservedFixedSlice(4);

			int offset = writer.Length;

			writer.WriteUInt16((ushort)header);

			composer.Compose(ref writer, packet);

			slice.WriteInt32(writer.Length - offset);
		}
		else
		{
			Console.WriteLine($"Missing composer: {typeof(T)}");
		}
	}
}
