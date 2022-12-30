using Skylight.API.Game.Badges;
using Skylight.API.Game.Inventory.Items;

namespace Skylight.API.Game.Inventory.Badges;

public interface IBadgeInventoryItem : IInventoryItem
{
	public IBadge Badge { get; }
}
