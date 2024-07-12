using Skylight.Domain.Rooms.Layout;

namespace Skylight.Domain.Rooms.Public;

public class PublicRoomWorldEntity
{
	public int RoomId { get; init; }
	public PublicRoomEntity? Room { get; set; }

	public int WorldId { get; init; }

	public string LayoutId { get; set; } = null!;
	public RoomLayoutEntity? Layout { get; set; }
}
