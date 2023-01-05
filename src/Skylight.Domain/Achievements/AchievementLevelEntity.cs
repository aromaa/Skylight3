using Skylight.Domain.Badges;

namespace Skylight.Domain.Achievements;

public class AchievementLevelEntity
{
	public int AchievementId { get; init; }
	public AchievementEntity? Achievement { get; set; }

	public int Level { get; init; }

	public string BadgeCode { get; set; } = null!;
	public BadgeEntity Badge { get; set; } = null!;

	public int ProgressRequirement { get; set; }
}
