using Skylight.Domain.Users;

namespace Skylight.Domain.Permissions;

public class UserRankEntity
{
	public int UserId { get; set; }
	public UserEntity? User { get; set; }

	public string RankId { get; set; } = null!;
	public RankEntity? Rank { get; set; }
}
