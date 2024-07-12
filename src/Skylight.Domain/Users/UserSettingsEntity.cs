using Skylight.Domain.Rooms.Private;

namespace Skylight.Domain.Users;

public class UserSettingsEntity
{
	public int UserId { get; init; }
	public UserEntity? User { get; set; }
	public PrivateRoomEntity? HomeRoom { get; set; }
	public int? HomeRoomId { get; set; }
}
