using Skylight.API.Game.Furniture.Floor;
using Skylight.API.Game.Inventory.Items.Floor;
using Skylight.API.Game.Users;

namespace Skylight.Server.Game.Inventory.Items.Floor;

internal abstract class FloorInventoryItem : IFloorInventoryItem
{
	public int Id { get; }
	public IUserInfo Owner { get; }

	public abstract IFloorFurniture Furniture { get; }

	internal FloorInventoryItem(int id, IUserInfo owner)
	{
		this.Id = id;
		this.Owner = owner;
	}

	public int StripId => -this.Id;
}
