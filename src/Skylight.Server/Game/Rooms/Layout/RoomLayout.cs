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

		(this.Size, bool normalizeHeightMap) = RoomLayout.GetSize(heightMap);
		(this.HeightMap, this.Tiles) = RoomLayout.ParseHeightMap(heightMap, this.Size, normalizeHeightMap);

		this.DoorLocation = new Point2D(doorX, doorY);
		this.DoorDirection = doorDirection;

		this.Items = [];
	}

	private static (string HeightMap, ImmutableArray2D<RoomLayoutTile> Tiles) ParseHeightMap(string heightMap, Point2D size, bool normalizeHeightMap)
	{
		StringBuilder? stringBuilder = normalizeHeightMap ? new StringBuilder(heightMap.Length) : null;

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
				else if (char.IsAsciiLetter(tile) && tile <= 'w')
				{
					tileHeight = tile - 'a' + 10;
				}
				else if (tile == '\r')
				{
					if (heightMap.Length > i && heightMap[i] == '\n')
					{
						i++;
					}

					break;
				}
				else if (tile == '\n')
				{
					break;
				}
				else
				{
					throw new ArgumentException($"Invalid char at [{x}, {y}], index {i}.");
				}

				if (x == 0 && y > 0)
				{
					stringBuilder?.Append('\r');
				}

				stringBuilder?.Append(tile);

				builder[x, y] = new RoomLayoutTile(tileHeight);
			}

			while (x < size.X)
			{
				if (x == 0 && y > 0)
				{
					stringBuilder?.Append('\r');
				}

				builder[x++, y] = new RoomLayoutTile(-100);
			}
		}

		return (stringBuilder?.ToString() ?? heightMap, builder.MoveToImmutable());
	}

	internal void AddItem(PublicRoomItemEntity item)
	{
		this.Items.Add(item);
	}

	private static (Point2D Size, bool NormalizeHeightMap) GetSize(string heightMap)
	{
		int width = 0;
		int height = 0;
		int emptyLines = 0;
		bool normalizeHeightMap = false;

		ReadOnlySpan<char> span = heightMap;
		while (true)
		{
			int index = span.IndexOfAny('\r', '\n');
			if (index == -1)
			{
				if (span.Length > 0)
				{
					height += emptyLines + 1;
				}
				else
				{
					normalizeHeightMap = true;
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
				normalizeHeightMap = true;
			}

			if (span.Length > index && span[index] == '\n')
			{
				index++;
				normalizeHeightMap = true;
			}

			emptyLines = 0;
			span = span[index..];
		}

		return (new Point2D(width, height), normalizeHeightMap);
	}
}
