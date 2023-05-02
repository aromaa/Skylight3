using Skylight.Domain.Rooms;

namespace Skylight.Domain.Users;

public class UserSettingsEntity
{
	public int UserId { get; init; }
	public UserEntity? User { get; set; }
	public RoomEntity? Room { get; set; }
	public int? HomeRoom { get; set; }
}
