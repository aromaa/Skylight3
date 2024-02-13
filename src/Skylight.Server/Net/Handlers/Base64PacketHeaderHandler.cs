using System.Globalization;
using System.IO.Pipelines;
using System.Numerics;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using Net.Buffers;
using Net.Communication.Incoming.Consumer;
using Net.Communication.Outgoing;
using Net.Sockets.Pipeline.Handler;
using Net.Sockets.Pipeline.Handler.Incoming;
using Net.Sockets.Pipeline.Handler.Outgoing;
using Skylight.Protocol.Extensions;
using Skylight.Protocol.Packets.Manager;
using Skylight.Protocol.Packets.Outgoing.Handshake;
using Skylight.Server.Net.Crypto;

namespace Skylight.Server.Net.Handlers;

internal sealed class Base64PacketHeaderHandler : IncomingBytesHandler, IOutgoingObjectHandler
{
	private readonly ILogger<Base64PacketHeaderHandler> logger;

	private readonly AbstractGamePacketManager packetManager;

	private uint currentPacketLength;

	private RC4? incomingHeaderDecoder;
	private RC4? incomingMessageDecoder;

	private Pipe? outgoingEncoderPipe;
	private RC4? outgoingHeaderEncoder;
	private RC4? outgoingMessageEncoder;

	private int incomingPaddingDecoder;
	private int outgoingPaddingEncoder;

	internal BigInteger Prime { get; }
	internal BigInteger Generator { get; }

	internal Base64PacketHeaderHandler(ILogger<Base64PacketHeaderHandler> logger, AbstractGamePacketManager packetManager, BigInteger prime, BigInteger generator)
	{
		this.logger = logger;

		this.packetManager = packetManager;

		this.Prime = prime;
		this.Generator = generator;
	}

	protected override void Decode(IPipelineHandlerContext context, ref PacketReader reader)
	{
		if (this.incomingHeaderDecoder is not null)
		{
			if (this.currentPacketLength == 0)
			{
				if (reader.Remaining < 6)
				{
					return;
				}

				PacketReader headerSliced = reader.Slice(6);
				PacketReader headerReader = this.incomingHeaderDecoder.Read(ref headerSliced);

				headerReader.Skip(1); //Random, nice one
				headerReader.TryReadBase64UInt32(3, out this.currentPacketLength);

				this.incomingHeaderDecoder.AdvanceReader(headerReader.UnreadSequence.End);
			}
		}
		else
		{
			//We haven't read the next packet length, wait for it
			if (this.currentPacketLength == 0 && !reader.TryReadBase64UInt32(3, out this.currentPacketLength))
			{
				return;
			}
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
		RC4? messageDecoder = this.incomingMessageDecoder;
		if (messageDecoder is not null)
		{
			reader = messageDecoder.Read(ref reader);
			reader.Skip(this.IterateTokenRandom(ref this.incomingPaddingDecoder));
		}

		uint header = reader.ReadBase64UInt32(2);

		if (this.packetManager.TryGetConsumer(header, out IIncomingPacketConsumer? consumer))
		{
			this.logger.LogDebug("Incoming: " + consumer.GetType().GetGenericArguments()[0]);

			consumer.Read(context, ref reader);

			if (reader.Readable)
			{
				this.logger.LogDebug($"Packet has stuff left: {header} ({reader.Remaining})");
			}
		}
		else
		{
			this.logger.LogDebug($"Unknown packet: {header}");
		}

		messageDecoder?.AdvanceReader(reader.UnreadSequence.End);
	}

	public void Handle<T>(IPipelineHandlerContext context, ref PacketWriter writer, in T packet)
	{
		if (this.packetManager.TryGetComposer<T>(out IOutgoingPacketComposer? composer, out uint header))
		{
			this.logger.LogDebug("Outgoing: " + typeof(T));

			if (this.outgoingEncoderPipe is not null && typeof(T) != typeof(CompleteDiffieHandshakeOutgoingPacket))
			{
				//Reserve space for the header
				PacketWriter headerSlice = writer.ReservedFixedSlice(6);

				int offset = writer.Length;

				{
					ReadOnlySpan<byte> padding = [0, 0, 0, 0, 0];

					PacketWriter packetWriter = new(this.outgoingEncoderPipe.Writer);
					packetWriter.WriteBytes(padding.Slice(0, this.IterateTokenRandom(ref this.outgoingPaddingEncoder)));
					WritePacket(ref packetWriter, composer, header, packet);
					packetWriter.Dispose();

					this.outgoingEncoderPipe.Reader.TryRead(out ReadResult readResult);

					foreach (ReadOnlyMemory<byte> readOnlyMemory in readResult.Buffer)
					{
						this.outgoingMessageEncoder!.Write(readOnlyMemory.Span, ref writer);
					}

					this.outgoingEncoderPipe.Reader.AdvanceTo(readResult.Buffer.End);
				}

				{
					PacketWriter packetWriter = new(this.outgoingEncoderPipe.Writer);
					packetWriter.WriteByte(0); //Ignored byte
					packetWriter.WriteBase64UInt32(3, (uint)(writer.Length - offset));
					packetWriter.Dispose();

					this.outgoingEncoderPipe.Reader.TryRead(out ReadResult readResult);

					foreach (ReadOnlyMemory<byte> readOnlyMemory in readResult.Buffer)
					{
						this.outgoingHeaderEncoder!.Write(readOnlyMemory.Span, ref headerSlice);
					}

					this.outgoingEncoderPipe.Reader.AdvanceTo(readResult.Buffer.End);
				}
			}
			else
			{
				WritePacket(ref writer, composer, header, packet);
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			static void WritePacket(ref PacketWriter writer, IOutgoingPacketComposer composer, uint header, in T packet)
			{
				writer.WriteBase64UInt32(2, header);

				composer.Compose(ref writer, packet);

				writer.WriteByte(1);
			}
		}
		else
		{
			this.logger.LogDebug($"Missing composer: {typeof(T)}");
		}
	}

	internal void EnableEncryption(BigInteger sharedKey)
	{
		byte[] rc4Table = sharedKey.ToByteArray(isUnsigned: true, isBigEndian: true);

		this.incomingHeaderDecoder = new RC4(rc4Table);
		this.incomingMessageDecoder = new RC4(rc4Table);

		this.outgoingEncoderPipe = new Pipe();
		this.outgoingHeaderEncoder = new RC4(rc4Table);
		this.outgoingMessageEncoder = new RC4(rc4Table);
	}

	internal void SetToken(BigInteger integer)
	{
		Span<char> chars = stackalloc char[64];

		if (integer.TryFormat(chars, out int writtenChars, "X"))
		{
			this.incomingPaddingDecoder = int.Parse(chars.Slice(Math.Max(0, writtenChars - 4), writtenChars), NumberStyles.AllowHexSpecifier);
			this.outgoingPaddingEncoder = int.Parse(chars.Slice(0, Math.Min(4, writtenChars)), NumberStyles.AllowHexSpecifier);
		}
		else
		{
			throw new FormatException();
		}
	}

	private int IterateTokenRandom(ref int token)
	{
		token = ((19979 * token) + 5) % (ushort.MaxValue + 1);

		return token % 5;
	}
}
