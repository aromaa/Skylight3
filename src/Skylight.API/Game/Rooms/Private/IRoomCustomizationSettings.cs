namespace Skylight.API.Game.Rooms.Private;

public interface IRoomCustomizationSettings
{
	public bool HideWalls { get; }

	public int FloorThickness { get; }
	public int WallThickness { get; }
}
