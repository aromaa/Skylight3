using Skylight.Server.Game.Rooms.Layout;

namespace Skylight.Server.Tests.Game.Rooms.Layout;

public class RoomLayoutTests
{
	[Theory]
	[InlineData("", 0, 0)]
	[InlineData("x", 1, 1)]
	[InlineData("x\r", 1, 1)]
	[InlineData("x\r\n", 1, 1)]
	[InlineData("x\n", 1, 1)]
	[InlineData("x\rx", 1, 2)]
	[InlineData("x\r\nx", 1, 2)]
	[InlineData("x\nx", 1, 2)]
	[InlineData("x\rxx\rx", 2, 3)]
	[InlineData("x\r\r", 1, 1)]
	[InlineData("x\r\rx", 1, 3)]
	[InlineData("x\r\rx\r\r", 1, 3)]
	[InlineData("x\r\rx\r\rx", 1, 5)]
	public void Parse_HeightMap_CorrectSize(string heightMap, int width, int height)
	{
		RoomLayout layout = new("test", heightMap, 0, 0, 0);

		Assert.Equal(width, layout.Size.X);
		Assert.Equal(height, layout.Size.Y);
	}

	[Theory]
	[InlineData("x", new[] { -1 })]
	[InlineData("X", new[] { -1 })]
	[InlineData("0123456789", new[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 })]
	[InlineData("abcdefghijklmnopqrstuvw", new[] { 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32 })]
	[InlineData("ABCDEFGHIJKLMNOPQRSTUVW", new[] { 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32 })]
	[InlineData("0\r00\r0", new[] { 0, -1 }, new[] { 0, 0 }, new[] { 0, -1 })]
	[InlineData("0\r\r0", new[] { 0 }, new[] { -1 }, new[] { 0 })]
	[InlineData("0\r\r00\r\r0", new[] { 0, -1 }, new[] { -1, -1 }, new[] { 0, 0 }, new[] { -1, -1 }, new[] { 0, -1 })]
	public void Parse_HeightMap_CorrectTiles(string heightMap, params int[][] tileHeightMap)
	{
		RoomLayout layout = new("test", heightMap, 0, 0, 0);

		for (int y = 0; y < tileHeightMap.Length; y++)
		{
			int[] tiles = tileHeightMap[y];
			for (int x = 0; x < tiles.Length; x++)
			{
				RoomLayoutTile tile = layout.Tiles[x, y];

				int height = tile.IsHole ? -1 : tile.Height;

				Assert.True(tiles[x] == height, $"Values differ at [{x}, {y}]. Expected: {tiles[x]}. Actual: {height}.");
			}
		}
	}

	[Theory]
	[InlineData("/")]
	[InlineData(":")]
	[InlineData("`")]
	[InlineData("y")]
	[InlineData("z")]
	[InlineData("{")]
	[InlineData("@")]
	[InlineData("Y")]
	[InlineData("Z")]
	[InlineData("[")]
	public void Parse_HeightMap_InvalidTiles(string heightMap)
	{
		Assert.Throws<ArgumentException>(() => new RoomLayout("test", heightMap, 0, 0, 0));
	}

	[Theory]
	[InlineData("x\r", "x")]
	[InlineData("x\r\n", "x")]
	[InlineData("x\n", "x")]
	[InlineData("x\rx", "x\rx")]
	[InlineData("x\r\nx", "x\rx")]
	[InlineData("x\nx", "x\rx")]
	public void Parse_HeightMap_NormalizeWorks(string heightMap, string normalizedHeightMap)
	{
		RoomLayout layout = new("test", heightMap, 0, 0, 0);

		Assert.Equal(normalizedHeightMap, layout.HeightMap);
	}
}
