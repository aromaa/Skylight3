using Skylight.API.Game.Furniture.Floor;
using Skylight.API.Game.Users;

namespace Skylight.Server.Game.Inventory.Items.Floor;

internal sealed class DefaultFloorInventoryItem : FloorInventoryItem
{
	public override IFloorFurniture Furniture { get; }

	internal DefaultFloorInventoryItem(int id, IUserInfo owner, IFloorFurniture furniture)
		: base(id, owner)
	{
		this.Furniture = furniture;
	}
}
