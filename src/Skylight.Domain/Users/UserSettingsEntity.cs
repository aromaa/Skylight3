namespace Skylight.Domain.Users;
public class UserSettingsEntity
{
	public int UserId { get; set; }
	public UserEntity? User { get; set; }
	public int Home { get; set; }
}
