using System.IO.Pipelines;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Net.Buffers;

namespace Skylight.Server.Net.Crypto;

internal sealed class RC4Base64 : RC4
{
	internal RC4Base64(byte[] key, string cryptoKey, string cryptoPremix)
		: base(RC4Base64.BuildTable(key, cryptoKey))
	{
		this.PremixTable(cryptoPremix, 52);
	}

	private byte MoveUp()
	{
		int q = ++this.Q;
		int j = this.J += this.Table[q];

		(this.Table[q], this.Table[j]) = (this.Table[j], this.Table[q]);

		if ((q & 0x3F) == 0x3F)
		{
			byte q2 = (byte)(0x129 * (q + 0x43));
			byte j2 = (byte)(j + this.Table[q2]);

			(this.Table[q2], this.Table[j2]) = (this.Table[j2], this.Table[q2]);
		}

		return this.Table[(byte)(this.Table[q] + this.Table[j])];
	}

	internal override void Write(ReadOnlySpan<byte> data, ref PacketWriter writer)
	{
		ref byte encodingMap = ref MemoryMarshal.GetReference("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/"u8);

		for (int i = 0; i < data.Length; i += 3)
		{
			int firstByte = data[i] ^ this.MoveUp();
			int secondByte = data.Length > i + 1 ? (data[i + 1] ^ this.MoveUp()) : 0;

			writer.WriteByte(Unsafe.Add(ref encodingMap, firstByte >> 2));
			writer.WriteByte(Unsafe.Add(ref encodingMap, ((firstByte & 0x3) << 4) | (secondByte >> 4)));

			if (data.Length > i + 1)
			{
				int thirdByte = data.Length > i + 2 ? (data[i + 2] ^ this.MoveUp()) : 0;

				writer.WriteByte(Unsafe.Add(ref encodingMap, ((secondByte & 0xF) << 2) | (thirdByte >> 6)));

				if (data.Length > i + 2)
				{
					writer.WriteByte(Unsafe.Add(ref encodingMap, thirdByte & 0x3F));
				}
			}
		}
	}

	internal override PacketReader Read(scoped ref PacketReader reader)
	{
		ReadOnlySpan<sbyte> decodingMapTable =
		[
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 62, -1, -1, -1, 63,
			52, 53, 54, 55, 56, 57, 58, 59, 60, 61, -1, -1, -1, -1, -1, -1,
			-1, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14,
			15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, -1, -1, -1, -1, -1,
			-1, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40,
			41, 42, 43, 44, 45, 46, 47, 48, 49, 50, 51, -1, -1, -1, -1, -1,
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

		//Vector128<int> shuffled = Avx2.Shuffle(examine.AsByte(), Vector128.Create(0, 255, 255, 255, 1, 255, 255, 255, 2, 255, 255, 255, 3, 255, 255, 255)).AsInt32();
		//Vector128<int> leftShift = Avx2.ShiftLeftLogicalVariable(shuffled, Vector128.Create(2u, 4, 6, 0));
		//Vector128<int> mask = Avx2.And(shuffled, Vector128.Create(0xFC, 0x30, 0x3C, 0x3F));
		//Vector128<int> rightShift = Avx2.ShiftRightLogical128BitLane(Avx2.ShiftRightLogicalVariable(mask, Vector128.Create(0u, 4, 2, 0)), 4);
		//Vector128<int> or = Avx2.Or(leftShift, rightShift);
		//Vector128<byte> final = Avx2.Shuffle(or.AsByte(), Vector128.Create(0, 4, 8, 12, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255));

		PacketWriter writer = new(this.DecodePipe.Writer);

		while (reader.Readable)
		{
			int firstByte = Unsafe.Add(ref decodingMap, (nuint)reader.ReadByte());
			int secondByte = Unsafe.Add(ref decodingMap, (nuint)reader.ReadByte());

			int byte1a = firstByte << 2;
			int byte1b = (secondByte & 0x30) >> 4;

			writer.WriteByte((byte)((byte1a | byte1b) ^ this.MoveUp()));

			if (reader.Readable)
			{
				int thirdByte = Unsafe.Add(ref decodingMap, (nuint)reader.ReadByte());

				int byte2a = secondByte << 4;
				int byte2b = (thirdByte & 0x3C) >> 2;

				writer.WriteByte((byte)((byte2a | byte2b) ^ this.MoveUp()));

				if (reader.Readable)
				{
					int fourthByte = Unsafe.Add(ref decodingMap, (nuint)reader.ReadByte());

					int byte3a = thirdByte << 6;
					int byte3b = fourthByte & 0x3F;

					writer.WriteByte((byte)((byte3a | byte3b) ^ this.MoveUp()));
				}
			}
		}

		writer.Dispose();

		if (this.DecodePipe.Reader.TryRead(out ReadResult result))
		{
			return new PacketReader(result.Buffer);
		}

		return default;
	}

	private void PremixTable(string data, int count)
	{
		for (int a = 0; a < count; a++)
		{
			for (int b = 0; b < data.Length; b++)
			{
				int result = data[b] ^ this.MoveUp();
			}
		}
	}

	private static byte[] BuildTable(byte[] key, string cryptoKey)
	{
		byte[] modKey = new byte[key.Length];
		for (int i = 0, j = 0; i < key.Length; i++, j++)
		{
			if (j >= cryptoKey.Length)
			{
				j = 0;
			}

			modKey[i] = (byte)(key[i] ^ cryptoKey[j]);
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
