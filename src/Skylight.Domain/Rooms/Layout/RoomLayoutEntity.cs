namespace Skylight.Domain.Rooms.Layout;

public class RoomLayoutEntity
{
	public required string Id { get; init; }

	public int DoorX { get; set; }
	public int DoorY { get; set; }
	public int DoorDirection { get; set; }

	public string HeightMap { get; set; } = null!;
}
