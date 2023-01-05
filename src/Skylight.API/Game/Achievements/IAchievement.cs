using System.Collections.Immutable;

namespace Skylight.API.Game.Achievements;

public interface IAchievement
{
	public int Id { get; }

	public string Category { get; }

	public bool DisplayProgress { get; }

	public ImmutableArray<IAchievementLevel> Levels { get; }
}
