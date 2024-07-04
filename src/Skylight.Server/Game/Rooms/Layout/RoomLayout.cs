using System.Text;
using Skylight.API.Game.Rooms.Map;
using Skylight.API.Numerics;
using Skylight.Domain.Items;
using Skylight.Server.Collections.Immutable;

namespace Skylight.Server.Game.Rooms.Layout;

internal sealed class RoomLayout : IRoomLayout
{
	public string Id { get; }

	public Point2D Size { get; }

	public string HeightMap { get; }

	public Point2D DoorLocation { get; }
	public int DoorDirection { get; }

	internal ImmutableArray2D<RoomLayoutTile> Tiles { get; }

	internal List<PublicRoomItemEntity> Items { get; }

	internal RoomLayout(string id, string heightMap, int doorX, int doorY, int doorDirection)
	{
		this.Id = id;

		(this.Size, bool fixHeightMapData) = RoomLayout.GetSize(heightMap);
		(this.HeightMap, this.Tiles) = RoomLayout.ParseHeightMap(heightMap, this.Size, fixHeightMapData);

		this.DoorLocation = new Point2D(doorX, doorY);
		this.DoorDirection = doorDirection;

		this.Items = [];
	}

	private static (string HeightMap, ImmutableArray2D<RoomLayoutTile> Tiles) ParseHeightMap(string heightMap, Point2D size, bool generateNewHeightMap)
	{
		StringBuilder? stringBuilder = generateNewHeightMap ? new StringBuilder(heightMap.Length) : null;

		ImmutableArray2D<RoomLayoutTile>.Builder builder = ImmutableArray2D.CreateBuilder<RoomLayoutTile>(size.X, size.Y);

		int i = 0;
		for (int y = 0; y < size.Y; y++)
		{
			int x = 0;
			for (; i < heightMap.Length; x++)
			{
				char tile = char.ToLowerInvariant(heightMap[i++]);

				int tileHeight;
				if (char.IsAsciiDigit(tile))
				{
					tileHeight = tile - '0';
				}
				else if (tile == 'x')
				{
					tileHeight = -100;
				}
				else if (char.IsAsciiLetter(tile))
				{
					tileHeight = tile - 'a' + 10;
				}
				else if (tile == '\r')
				{
					break;
				}
				else if (tile == '\n')
				{
					x--; //Ignore new line

					continue;
				}
				else
				{
					throw new NotSupportedException("wtf, u idiot");
				}

				stringBuilder?.Append(tile);

				builder[x, y] = new RoomLayoutTile(tileHeight);
			}

			while (x < size.X)
			{
				builder[x++, y] = new RoomLayoutTile(-100);
			}

			stringBuilder?.Append('\r');
		}

		return (stringBuilder?.ToString() ?? heightMap, builder.MoveToImmutable());
	}

	internal void AddItem(PublicRoomItemEntity item)
	{
		this.Items.Add(item);
	}

	private static (Point2D Size, bool FixHeightMapData) GetSize(string heightMap)
	{
		int width = 0;
		int height = 0;
		int emptyLines = 0;
		bool fixHeightMapData = false;

		ReadOnlySpan<char> span = heightMap;
		while (!span.IsEmpty)
		{
			int index = span.IndexOfAny('\r', '\n');
			if (index == -1)
			{
				if (span.Length > 0)
				{
					height += emptyLines + 1;
				}

				width = int.Max(width, span.Length);

				break;
			}
			else if (index == 0)
			{
				emptyLines++;
				span = span[1..];

				continue;
			}

			height += emptyLines + 1;
			width = int.Max(width, index);

			if (span[index] == '\r')
			{
				index++;
			}
			else
			{
				fixHeightMapData = true;
			}

			if (span.Length > index && span[index] == '\n')
			{
				index++;
				fixHeightMapData = true;
			}

			emptyLines = 0;
			span = span[index..];
		}

		return (new Point2D(width, height), fixHeightMapData);
	}
}
