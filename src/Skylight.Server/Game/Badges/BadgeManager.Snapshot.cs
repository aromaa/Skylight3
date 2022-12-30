using System.Diagnostics.CodeAnalysis;
using Skylight.API.Game.Badges;

namespace Skylight.Server.Game.Badges;

internal partial class BadgeManager
{
	public bool TryGetBadge(string badgeCode, [NotNullWhen(true)] out IBadge? badge) => this.snapshot.TryGetBadge(badgeCode, out badge);

	private sealed class Snapshot : IBadgeSnapshot
	{
		private readonly Cache cache;

		internal Snapshot(Cache cache)
		{
			this.cache = cache;
		}

		public bool TryGetBadge(string badgeCode, [NotNullWhen(true)] out IBadge? badge) => this.cache.Badges.TryGetValue(badgeCode, out badge);
	}
}
