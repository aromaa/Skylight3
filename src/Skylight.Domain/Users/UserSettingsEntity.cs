namespace Skylight.Domain.Users;
public class UserSettingsEntity
{
	public int UserId { get; }
	public UserEntity? User { get; set; }
	public int Home { get; set; }
}
