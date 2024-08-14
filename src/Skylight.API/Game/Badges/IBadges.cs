using System.Diagnostics.CodeAnalysis;

namespace Skylight.API.Game.Badges;

public interface IBadges
{
	public bool TryGetBadge(string badgeCode, [NotNullWhen(true)] out IBadge? badge);
}
