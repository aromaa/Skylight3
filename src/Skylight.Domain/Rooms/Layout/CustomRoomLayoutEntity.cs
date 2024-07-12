namespace Skylight.Domain.Rooms.Layout;

public class CustomRoomLayoutEntity
{
	public int RoomId { get; init; }
	public RoomEntity? Room { get; init; }

	public int DoorX { get; set; }
	public int DoorY { get; set; }
	public int DoorDirection { get; set; }

	public string HeightMap { get; set; } = null!;
}
