namespace Skylight.Domain.Rooms.Private;

public class PrivateRoomActivityEntity
{
	public int RoomId { get; init; }
	public PrivateRoomEntity? Room { get; init; }

	public int Day { get; init; }

	public int Value { get; set; }
}
