namespace Skylight.Domain.Achievements;

public class AchievementEntity
{
	public int Id { get; init; }

	public string Category { get; set; } = null!;

	public bool DisplayProgress { get; set; }

	public List<AchievementLevelEntity>? Levels { get; set; }
}
