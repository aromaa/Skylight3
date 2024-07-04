using Skylight.Server.Game.Rooms.Layout;

namespace Skylight.Server.Tests.Game.Rooms.Layout;

public class RoomLayoutTests
{
	[Theory]
	[InlineData("", 0, 0)]
	[InlineData("x", 1, 1)]
	[InlineData("x\r", 1, 1)]
	[InlineData("x\r\n", 1, 1)]
	//[InlineData("x\n", 1, 1)]
	[InlineData("x\rx", 1, 2)]
	[InlineData("x\r\nx", 1, 2)]
	//[InlineData("x\nx", 1, 2)]
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
}
