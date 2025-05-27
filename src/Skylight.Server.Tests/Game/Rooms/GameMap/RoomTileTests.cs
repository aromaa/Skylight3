﻿using Moq;
using Skylight.API.Game.Furniture.Floor;
using Skylight.API.Game.Rooms.Items.Floor;
using Skylight.API.Game.Rooms.Map;
using Skylight.API.Numerics;
using Skylight.API.Registry;
using Skylight.Server.Game.Furniture.Floor;
using Skylight.Server.Game.Rooms.Layout;
using Skylight.Server.Game.Rooms.Map;
using Skylight.Server.Game.Rooms.Map.Private;
using Skylight.Server.Registry;
using Skylight.Server.Tests.Registry;

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
		FloorFurnitureKind walkable = new();

		DummyRegistryHolder registryHolder = new(Registry<IFloorFurnitureKindType>.Create(
			RegistryTypes.FloorFurnitureKind,
			(FloorFurnitureKindTypes.Walkable.Key, new FloorFurnitureKindType(walkable))));

		yield return [CreateTile(), 123, 0, 0, 0];
		yield return [CreateTile((123, 0)), 123, 0, 0, 123];
		yield return [CreateTile((123, 0)), 23, 100, 0, 123];
		yield return [CreateTile((123, 0.5)), 123, 0.5, 0.5, 123.5];
		yield return [CreateTile((123, 0), (124, 0)), 123, 0, 1, 123];
		yield return [CreateTile((123, 0.5), (124, 0.5)), 123, 0.5, 0.5, 123.5];
		yield return [CreateTile((122, 0), (124, 0)), 123, 1, 0, 124];
		yield return [CreateTile((120, 0), (126, 0)), 123, 10, 100, 126];
		yield return [CreateTile((122.1, 0), (124, 0)), 123, 1, 0, 124];
		yield return [CreateTile((123, 0), (123.99, 0)), 123, 1, 1, 123.99];
		yield return [CreateTile((123, 3), (124, 0)), 124, 10, 1, 124];
		yield return [CreateTile((123, 0)), double.BitDecrement(123), 0, 0, 0];
		yield return [CreateTile((123, 0)), double.BitIncrement(123), 0, 0, 123];

		RoomTile CreateTile(params (double Z, double Height)[] slices)
		{
			Point2D location = new(0, 0);

			Mock<IRoomMap> roomMapMock = new();

			PrivateRoomTile rangeMap = new(null!, roomMapMock.Object, registryHolder, location, new RoomLayoutTile(0));

			foreach ((double z, double height) in slices)
			{
				Mock<IFloorRoomItem> itemMock = new();
				itemMock.SetupGet(i => i.Furniture.Kind).Returns(walkable);
				itemMock.SetupGet(i => i.Position).Returns(new Point3D(location, z));
				itemMock.SetupGet(i => i.Height).Returns(height);

				rangeMap.AddItem(itemMock.Object);
			}

			return rangeMap;
		}
	}
}
