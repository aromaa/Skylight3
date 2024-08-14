namespace Skylight.API.Game.Achievements;

public interface IAchievements
{
	public IEnumerable<IAchievement> Achievements { get; }

	public IEnumerable<KeyValuePair<string, int>> BadgePointLimits { get; }
}
