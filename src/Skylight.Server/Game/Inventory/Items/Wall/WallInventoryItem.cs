using Skylight.API.Game.Furniture.Wall;
using Skylight.API.Game.Inventory.Items.Wall;
using Skylight.API.Game.Users;

namespace Skylight.Server.Game.Inventory.Items.Wall;

internal abstract class WallInventoryItem : IWallInventoryItem
{
	public int Id { get; }
	public IUserInfo Owner { get; }

	public abstract IWallFurniture Furniture { get; }

	internal WallInventoryItem(int id, IUserInfo owner)
	{
		this.Id = id;
		this.Owner = owner;
	}

	public int StripId => this.Id;
}
