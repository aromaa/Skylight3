using Moq;
using Skylight.API.Game.Rooms.Items.Floor;
using Skylight.API.Game.Rooms.Map;
using Skylight.API.Numerics;
using Skylight.Server.Game.Rooms.GameMap;
using Skylight.Server.Game.Rooms.Layout;

namespace Skylight.Server.Tests.Game.Rooms.GameMap;

public class RoomTileTests
{
	[Theory]
	[MemberData(nameof(RoomTileTests.StepHeightData))]
	internal static void FindStepHeight(RoomTile tile, double target, double range, double emptySpace, double expected)
	{
		Assert.Equal(expected, tile.GetStepHeight(target, range, emptySpace));
	}

	public static IEnumerable<object[]> StepHeightData()
	{
		yield return [CreateTile(), 123, 0, 0, 0];
		yield return [CreateTile((123, 0)), 123, 0, 0, 123];
		yield return [CreateTile((123, 0)), 23, 0, 100, 123];
		yield return [CreateTile((123, 0.5)), 123, 0, 0.5, 123.5];
		yield return [CreateTile((123, 0), (124, 0)), 123, 0, 1, 123];
		yield return [CreateTile((123, 0.5), (124, 0.5)), 123, 0.5, 0.5, 123.5];
		yield return [CreateTile((122, 0), (124, 0)), 123, 1, 0, 124];
		yield return [CreateTile((120, 0), (126, 0)), 123, 10, 100, 126];
		yield return [CreateTile((122.1, 0), (124, 0)), 123, 1, 0, 124];
		yield return [CreateTile((123, 0), (123.99, 0)), 123, 0, 1, 123.99];
		yield return [CreateTile((123, 3), (124, 0)), 124, 10, 1, 124];
		yield return [CreateTile((123, 0)), double.BitDecrement(123), 0, 0, 123];
		yield return [CreateTile((123, 0)), double.BitIncrement(123), 0, 0, 123];

		static RoomTile CreateTile(params (double Z, double Height)[] slices)
		{
			Point2D location = new(0, 0);

			Mock<IRoomMap> roomMapMock = new();

			RoomTile rangeMap = new(null!, roomMapMock.Object, location, new RoomLayoutTile(0));

			foreach ((double z, double height) in slices)
			{
				Mock<IFloorRoomItem> itemMock = new();
				itemMock.SetupGet(i => i.Position).Returns(new Point3D(location, z));
				itemMock.SetupGet(i => i.Height).Returns(height);

				rangeMap.AddItem(itemMock.Object);
			}

			return rangeMap;
		}
	}
}
