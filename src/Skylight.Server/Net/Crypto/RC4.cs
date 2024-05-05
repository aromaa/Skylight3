using System.IO.Pipelines;
using Net.Buffers;

namespace Skylight.Server.Net.Crypto;

internal abstract class RC4
{
	protected byte Q { get; set; }
	protected byte J { get; set; }

	protected byte[] Table { get; }

	protected Pipe DecodePipe { get; }

	internal RC4(byte[] table)
	{
		this.Table = table;

		this.DecodePipe = new Pipe();
	}

	internal abstract PacketReader Read(scoped ref PacketReader reader);
	internal abstract void Write(ReadOnlySpan<byte> data, ref PacketWriter writer);

	internal void AdvanceReader(SequencePosition consumed) => this.DecodePipe.Reader.AdvanceTo(consumed);
}
