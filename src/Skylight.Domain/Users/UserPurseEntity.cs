namespace Skylight.Domain.Users;

public class UserPurseEntity
{
	public int UserId { get; set; }
	public UserEntity? User { get; set; }

	public string Currency { get; set; } = null!;
	public int Balance { get; set; }
}
