using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Text.RegularExpressions;
using CommunityToolkit.HighPerformance;
using Skylight.API.Game.Achievements;
using Skylight.API.Game.Badges;
using Skylight.Domain.Achievements;

namespace Skylight.Server.Game.Achievements;

internal partial class AchievementManager
{
	private sealed partial class Cache
	{
		internal FrozenDictionary<int, IAchievement> Achievements { get; }

		internal FrozenDictionary<string, int> BadgePointLimits { get; }

		internal Cache(Dictionary<int, IAchievement> achievements, Dictionary<string, int> badgePointLimits)
		{
			this.Achievements = achievements.ToFrozenDictionary(optimizeForReading: true);

			this.BadgePointLimits = badgePointLimits.ToFrozenDictionary(optimizeForReading: true);
		}

		internal static Builder CreateBuilder() => new();

		internal sealed class Builder
		{
			private readonly Dictionary<int, AchievementEntity> achievements;

			internal Builder()
			{
				this.achievements = new Dictionary<int, AchievementEntity>();
			}

			internal void AddAchievement(AchievementEntity achievement)
			{
				this.achievements.Add(achievement.Id, achievement);
			}

			internal Cache ToImmutable(IBadgeSnapshot badges)
			{
				Dictionary<int, IAchievement> achievements = new();
				Dictionary<string, int> badgePointLimits = new();

				foreach (AchievementEntity achievementEntity in this.achievements.Values)
				{
					if (achievementEntity.Levels is not { Count: > 0 })
					{
						throw new InvalidOperationException($"The achievement {achievementEntity.Id} has no levels!");
					}

					ImmutableArray<IAchievementLevel>.Builder levels = ImmutableArray.CreateBuilder<IAchievementLevel>(achievementEntity.Levels.Count);

					int i = 1;
					string? lastBadgeCode = null;

					IAchievementLevel? previousLevel = null;
					Ref<IAchievementLevel?> nextLevel = default;
					foreach (AchievementLevelEntity levelEntity in achievementEntity.Levels)
					{
						if (levelEntity.Level != i)
						{
							throw new InvalidOperationException($"The achievement {achievementEntity.Id} has a level {levelEntity.Level} but it should be {i}!");
						}

						Match match = Cache.ParseAchievementBadge().Match(levelEntity.BadgeCode);
						if (!match.Success)
						{
							throw new InvalidOperationException($"The achievement {achievementEntity.Id} has a level {levelEntity.Level} with a badge code {levelEntity.BadgeCode}. Achievement badge codes must start with ACH_ and end with the level number.");
						}

						if (lastBadgeCode is not null && !match.Groups[1].ValueSpan.SequenceEqual(lastBadgeCode))
						{
							throw new InvalidOperationException($"The achievement {achievementEntity.Id} has a level {levelEntity.Level} with a badge code {levelEntity.BadgeCode} that doesn't match with {lastBadgeCode}!");
						}

						if (int.Parse(match.Groups[2].ValueSpan) != i)
						{
							throw new InvalidOperationException($"The achievement {achievementEntity.Id} has a level {levelEntity.Level} with a badge code {levelEntity.BadgeCode} that doesn't end with {i}!");
						}

						if (!badges.TryGetBadge(levelEntity.BadgeCode, out IBadge? badge))
						{
							throw new InvalidOperationException($"The achievement {achievementEntity.Id} has a level {levelEntity.Level} with badge code {levelEntity.BadgeCode} but it doesn't exist!");
						}

						i++;
						lastBadgeCode = match.Groups[1].Value;

						levels.Add(previousLevel = new AchievementLevel(levelEntity.Level, badge, levelEntity.ProgressRequirement, previousLevel, ref nextLevel));
						badgePointLimits.Add(levelEntity.BadgeCode, levelEntity.ProgressRequirement);
					}

					achievements.Add(achievementEntity.Id, new Achievement(achievementEntity.Id, achievementEntity.Category, achievementEntity.DisplayProgress, levels.MoveToImmutable()));
				}

				return new Cache(achievements, badgePointLimits);
			}
		}

		[GeneratedRegex("ACH_(.+?)([0-9]+)$")]
		private static partial Regex ParseAchievementBadge();
	}
}
