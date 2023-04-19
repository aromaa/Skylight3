namespace Skylight.Domain.Users;
public class UserSettingsEntity
{
	public int UserId { get; init; }
	public UserEntity? User { get; set; }
	public int Home { get; set; }
}
