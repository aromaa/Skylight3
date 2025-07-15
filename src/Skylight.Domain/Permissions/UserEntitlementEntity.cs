using Skylight.Domain.Users;

namespace Skylight.Domain.Permissions;

public class UserEntitlementEntity
{
	public int UserId { get; set; }
	public UserEntity? User { get; set; }

	public string Entitlement { get; set; } = null!;
	public string Value { get; set; } = null!;
}
