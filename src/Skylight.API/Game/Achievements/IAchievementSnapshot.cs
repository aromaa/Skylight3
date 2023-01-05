namespace Skylight.API.Game.Achievements;

public interface IAchievementSnapshot
{
	public IEnumerable<IAchievement> Achievements { get; }

	public IEnumerable<KeyValuePair<string, int>> BadgePointLimits { get; }
}
