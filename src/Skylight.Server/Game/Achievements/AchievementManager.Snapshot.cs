using Skylight.API.Game.Achievements;

namespace Skylight.Server.Game.Achievements;

internal partial class AchievementManager
{
	public IEnumerable<IAchievement> Achievements => this.snapshot.Achievements;

	public IEnumerable<KeyValuePair<string, int>> BadgePointLimits => this.snapshot.BadgePointLimits;

	private sealed class Snapshot : IAchievementSnapshot
	{
		private readonly Cache cache;

		internal Snapshot(Cache cache)
		{
			this.cache = cache;
		}

		public IEnumerable<IAchievement> Achievements => this.cache.Achievements.Values;

		public IEnumerable<KeyValuePair<string, int>> BadgePointLimits => this.cache.BadgePointLimits;
	}
}
