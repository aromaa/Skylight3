using Skylight.Domain.Users;

namespace Skylight.Domain.Badges;

public class UserBadgeEntity
{
	public int UserId { get; init; }
	public UserEntity? User { get; set; }

	public string BadgeCode { get; set; } = null!;
	public BadgeEntity? Badge { get; set; }
}
