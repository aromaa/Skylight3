namespace Skylight.Domain.Rooms.Private;

public class PrivateRoomActivityEntity
{
	public int RoomId { get; init; }
	public PrivateRoomEntity? Room { get; init; }

	public int Week { get; init; }
	public int Total { get; private set; }

	public int Monday { get; set; }
	public int Tuesday { get; set; }
	public int Wednesday { get; set; }
	public int Thursday { get; set; }
	public int Friday { get; set; }
	public int Saturday { get; set; }
	public int Sunday { get; set; }
}
