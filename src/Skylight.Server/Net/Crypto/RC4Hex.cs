using System.IO.Pipelines;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Net.Buffers;

namespace Skylight.Server.Net.Crypto;

internal sealed class RC4Hex : RC4
{
	private bool fool;

	internal RC4Hex(string cryptoKey, string cryptoPremix)
		: base(RC4Hex.BuildTable(cryptoKey))
	{
		this.PremixTable(cryptoPremix, 17);
	}

	internal RC4Hex(byte[] key, string? cryptoKey, string cryptoPremix)
		: base(RC4Hex.BuildTable(key, cryptoKey))
	{
		this.fool = cryptoKey is not null;

		this.PremixTable(cryptoPremix, cryptoKey is null ? 17 : 52);
	}

	private byte MoveUp()
	{
		int q = ++this.Q;
		int j = this.J += this.Table[q];

		(this.Table[q], this.Table[j]) = (this.Table[j], this.Table[q]);

		if (this.fool)
		{
			byte i = (byte)(17 * (q + 19));
			byte j_ = (byte)(j + this.Table[i]);

			(this.Table[i], this.Table[j_]) = (this.Table[j_], this.Table[i]);

			if (q is 46 or 67 or 192)
			{
				byte q2 = (byte)(0x129 * (i + 0x43));
				byte j2 = (byte)(j_ + this.Table[q2]);

				(this.Table[q2], this.Table[j2]) = (this.Table[j2], this.Table[q2]);
			}
		}

		return this.Table[(byte)(this.Table[q] + this.Table[j])];
	}

	internal override PacketReader Read(scoped ref PacketReader reader)
	{
		ReadOnlySpan<sbyte> decodingMapTable =
		[
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			0, 1, 2, 3, 4, 5, 6, 7, 8, 9, -1, -1, -1, -1, -1, -1,
			-1, 10, 11, 12, 13, 14, 15, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
		];

		ref sbyte decodingMap = ref MemoryMarshal.GetReference(decodingMapTable);

		PacketWriter writer = new(this.DecodePipe.Writer);

		while (reader.Readable)
		{
			int firstByte = Unsafe.Add(ref decodingMap, (nuint)reader.ReadByte());
			int secondByte = Unsafe.Add(ref decodingMap, (nuint)reader.ReadByte());

			writer.WriteByte((byte)((firstByte << 4 | secondByte) ^ this.MoveUp()));
		}

		writer.Dispose();

		if (this.DecodePipe.Reader.TryRead(out ReadResult result))
		{
			return new PacketReader(result.Buffer);
		}

		return default;
	}

	internal override void Write(ReadOnlySpan<byte> data, ref PacketWriter writer) => throw new NotImplementedException();

	internal override void PremixTable(string data, int count)
	{
		for (int a = 0; a < count; a++)
		{
			for (int b = 0; b < data.Length; b++)
			{
				int result = data[b] ^ this.MoveUp();
			}
		}
	}

	private static byte[] BuildTable(string cryptoKey)
	{
		byte[] table = new byte[byte.MaxValue + 1];
		for (int i = 0; i <= byte.MaxValue; i++)
		{
			table[i] = (byte)i;
		}

		byte[] modKey = Convert.FromBase64String(cryptoKey);

		return RC4Hex.BuildTable(modKey);
	}

	private static byte[] BuildTable(byte[] key, string? cryptoKey = null)
	{
		byte[] modKey = key;
		if (cryptoKey is not null)
		{
			modKey = new byte[key.Length];
			for (int i = 0, j = 0; i < key.Length; i++, j++)
			{
				if (j >= cryptoKey.Length)
				{
					j = 0;
				}

				modKey[i] = (byte)(key[i] ^ cryptoKey[j]);
			}
		}

		byte[] table = new byte[byte.MaxValue + 1];
		for (int i = 0; i <= byte.MaxValue; i++)
		{
			table[i] = (byte)i;
		}

		for (int q = 0, j = 0; q <= byte.MaxValue; q++)
		{
			j = (j + table[q] + modKey[q % modKey.Length]) % (byte.MaxValue + 1);

			(table[q], table[j]) = (table[j], table[q]);
		}

		return table;
	}
}
