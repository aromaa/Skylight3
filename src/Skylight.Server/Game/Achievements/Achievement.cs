using System.Collections.Immutable;
using Skylight.API.Game.Achievements;

namespace Skylight.Server.Game.Achievements;

internal sealed class Achievement : IAchievement
{
	public int Id { get; }

	public string Category { get; }

	public bool DisplayProgress { get; }

	public short State { get; }

	public ImmutableArray<IAchievementLevel> Levels { get; }

	internal Achievement(int id, string category, bool displayProgress, short state, ImmutableArray<IAchievementLevel> levels)
	{
		this.Id = id;

		this.Category = category;

		this.DisplayProgress = displayProgress;

		this.State = state;

		this.Levels = levels;
	}
}
