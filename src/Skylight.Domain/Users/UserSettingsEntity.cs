using Skylight.Domain.Rooms;

namespace Skylight.Domain.Users;

public class UserSettingsEntity
{
	public int UserId { get; init; }
	public UserEntity? User { get; set; }
	public RoomEntity? HomeRoom { get; set; }
	public int? HomeRoomId { get; set; }
}
