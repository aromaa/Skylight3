using Skylight.API.Game.Rooms.Private;

namespace Skylight.Server.Game.Rooms.Private;

internal sealed class PrivateRoomCustomizationSettings(bool hideWalls, int floorThickness, int wallThickness) : IRoomCustomizationSettings
{
	public bool HideWalls { get; } = hideWalls;

	public int FloorThickness { get; } = floorThickness;
	public int WallThickness { get; } = wallThickness;
}
