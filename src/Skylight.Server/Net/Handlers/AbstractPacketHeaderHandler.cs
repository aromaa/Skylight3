using Net.Buffers;
using Net.Communication.Outgoing;
using Net.Sockets.Pipeline.Handler;

namespace Skylight.Server.Net.Handlers;

internal abstract class AbstractPacketHeaderHandler : PacketManagerHandler
{
	private uint currentPacketLength;

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

		ushort header = readerSliced.ReadUInt16();

		this.Read(context, header, ref readerSliced);

		this.currentPacketLength = 0;
	}

	private protected override void Handle<T>(IOutgoingPacketComposer composer, ref PacketWriter writer, uint header, in T packet)
	{
		PacketWriter slice = writer.ReservedFixedSlice(4);

		int offset = writer.Length;

		writer.WriteUInt16((ushort)header);

		composer.Compose(ref writer, packet);

		slice.WriteInt32(writer.Length - offset);
	}
}
