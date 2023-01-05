using Skylight.API.Game.Badges;

namespace Skylight.API.Game.Achievements;

public interface IAchievementLevel
{
	public int Level { get; }

	public IBadge Badge { get; }

	public int ProgressRequirement { get; }

	public IAchievementLevel? PreviousLevel { get; }
	public IAchievementLevel? NextLevel { get; }
}
