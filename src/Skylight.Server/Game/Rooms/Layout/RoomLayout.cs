using Skylight.API.Game.Rooms.Map;
using Skylight.API.Numerics;
using Skylight.Domain.Items;
using Skylight.Domain.Rooms.Layout;
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

	internal RoomLayout(RoomLayoutEntity layout)
	{
		this.Id = layout.Id;

		(int width, int height, bool hasNewLine) = RoomLayout.GetSize(layout.HeightMap);

		this.Size = new Point2D(width, height);

		this.HeightMap = hasNewLine ? layout.HeightMap.Replace("\n", string.Empty) : layout.HeightMap;

		this.DoorLocation = new Point2D(layout.DoorX, layout.DoorY);
		this.DoorDirection = layout.DoorDirection;

		this.Tiles = RoomLayout.ParseHeightMap(layout.HeightMap, width, height);

		this.Items = new List<PublicRoomItemEntity>();
	}

	private static ImmutableArray2D<RoomLayoutTile> ParseHeightMap(string heightMap, int width, int height)
	{
		ImmutableArray2D<RoomLayoutTile>.Builder builder = ImmutableArray2D.CreateBuilder<RoomLayoutTile>(width, height);

		int i = 0;
		for (int y = 0; y < height; y++)
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
					tileHeight = (tile - 'a') + 10;
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

				builder[x, y] = new RoomLayoutTile(tileHeight);
			}

			while (x < width)
			{
				builder[x++, y] = new RoomLayoutTile(-100);
			}
		}

		return builder.MoveToImmutable();
	}

	internal void AddItem(PublicRoomItemEntity item)
	{
		this.Items.Add(item);
	}

	private static (int Width, int Height, bool HasNewLine) GetSize(string heightMap)
	{
		int lastIndex = 0;
		int width = 0;
		int height = 0;
		bool hasNewLine = false;

		while (true)
		{
			height++;

			int index = heightMap.IndexOf('\r', lastIndex);
			if (index == -1)
			{
				int distance = heightMap.Length - lastIndex;
				if (distance > width)
				{
					width = distance;
				}

				break;
			}
			else
			{
				int distance = index - lastIndex;
				if (distance > width)
				{
					width = distance;
				}

				lastIndex = index + 1;

				//In case there is a new line
				if (heightMap[lastIndex] == '\n')
				{
					lastIndex++;
					hasNewLine = true;
				}
			}
		}

		return (width, height, hasNewLine);
	}
}
