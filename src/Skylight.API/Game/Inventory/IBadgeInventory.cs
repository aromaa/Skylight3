using System.Diagnostics.CodeAnalysis;
using Skylight.API.Game.Inventory.Badges;

namespace Skylight.API.Game.Inventory;

public interface IBadgeInventory
{
	public IEnumerable<IBadgeInventoryItem> Badges { get; }

	public bool TryAddBadge(IBadgeInventoryItem badge);

	public bool HasBadge(string badgeCode);

	public bool TryGetBadge(string badgeCode, [NotNullWhen(true)] out IBadgeInventoryItem? badge);
}
