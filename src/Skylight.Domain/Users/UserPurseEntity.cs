namespace Skylight.Domain.Users;

public class UserPurseEntity
{
	public int Id { get; init; }

	public int UserId { get; set; }
	public UserEntity? User { get; set; }

	public string CurrencyType { get; set; } = null!;
	public string? CurrencyData { get; set; }

	public int Balance { get; set; }
}
