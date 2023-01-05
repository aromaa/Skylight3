using System.Runtime.CompilerServices;
using CommunityToolkit.HighPerformance;
using Skylight.API.Game.Achievements;
using Skylight.API.Game.Badges;

namespace Skylight.Server.Game.Achievements;

internal sealed class AchievementLevel : IAchievementLevel
{
	public int Level { get; }

	public IBadge Badge { get; }

	public int ProgressRequirement { get; }

	private readonly IAchievementLevel? previousLevel;
	private readonly IAchievementLevel? nextLevel;

	internal AchievementLevel(int level, IBadge badge, int progressRequirement, IAchievementLevel? previousLevel, ref Ref<IAchievementLevel?> nextLevel)
	{
		this.Level = level;

		this.Badge = badge;

		this.ProgressRequirement = progressRequirement;

		this.previousLevel = previousLevel;

		ref IAchievementLevel? lastLevel = ref nextLevel.Value;
		if (!Unsafe.IsNullRef(ref lastLevel))
		{
			lastLevel = this;
		}

		nextLevel = new Ref<IAchievementLevel?>(ref this.nextLevel);
	}

	public IAchievementLevel? PreviousLevel => this.previousLevel;
	public IAchievementLevel? NextLevel => this.nextLevel;
}
