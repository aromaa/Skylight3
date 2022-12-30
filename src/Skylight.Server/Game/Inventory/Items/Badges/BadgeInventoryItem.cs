using Skylight.API.Game.Badges;
using Skylight.API.Game.Inventory.Badges;
using Skylight.API.Game.Users;

namespace Skylight.Server.Game.Inventory.Items.Badges;

internal sealed class BadgeInventoryItem : IBadgeInventoryItem
{
	public IBadge Badge { get; }

	public IUserInfo Owner { get; }

	internal BadgeInventoryItem(IBadge badge, IUserInfo owner)
	{
		this.Badge = badge;

		this.Owner = owner;
	}
}
