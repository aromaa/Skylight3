using Skylight.API.Game.Furniture.Wall;
using Skylight.API.Game.Users;

namespace Skylight.Server.Game.Inventory.Items.Wall;

internal sealed class DefaultWallInventoryItem : WallInventoryItem
{
	public override IWallFurniture Furniture { get; }

	internal DefaultWallInventoryItem(int id, IUserInfo owner, IWallFurniture furniture)
		: base(id, owner)
	{
		this.Furniture = furniture;
	}
}
