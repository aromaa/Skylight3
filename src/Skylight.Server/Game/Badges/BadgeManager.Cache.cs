using System.Collections.Frozen;
using Skylight.API.Game.Badges;
using Skylight.Domain.Badges;

namespace Skylight.Server.Game.Badges;

internal partial class BadgeManager
{
	private sealed class Cache
	{
		internal FrozenDictionary<string, IBadge> Badges { get; }

		private Cache(Dictionary<string, IBadge> badges)
		{
			this.Badges = badges.ToFrozenDictionary();
		}

		internal static Builder CreateBuilder() => new();

		internal sealed class Builder
		{
			private readonly Dictionary<int, BadgeEntity> badges;

			internal Builder()
			{
				this.badges = new Dictionary<int, BadgeEntity>();
			}

			internal void AddBadge(BadgeEntity badge)
			{
				this.badges.Add(badge.Id, badge);
			}

			internal Cache ToImmutable()
			{
				return new Cache(this.badges.Values.Select(b => new Badge(b.Id, b.Code)).ToDictionary<IBadge, string>(b => b.Code));
			}
		}
	}
}
