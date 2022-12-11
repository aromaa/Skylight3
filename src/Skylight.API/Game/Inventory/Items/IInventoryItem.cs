using Skylight.API.Game.Users;

namespace Skylight.API.Game.Inventory.Items;

public interface IInventoryItem
{
	public int Id { get; }

	public IUserInfo Owner { get; }
}
