using Skylight.Domain.Users;

namespace Skylight.Domain.Permissions;

public class UserPermissionEntity
{
	public int UserId { get; set; }
	public UserEntity? User { get; set; }

	public string Permission { get; set; } = null!;
	public bool Value { get; set; }
}
