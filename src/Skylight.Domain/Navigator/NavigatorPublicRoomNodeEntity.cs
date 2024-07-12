using Skylight.Domain.Rooms.Public;

namespace Skylight.Domain.Navigator;

public class NavigatorPublicRoomNodeEntity : NavigatorNodeEntity
{
	public int RoomId { get; set; }
	public int WorldId { get; set; }
	public PublicRoomWorldEntity? PublicRoomWorld { get; set; }
}
