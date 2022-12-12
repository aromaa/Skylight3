using Skylight.API.Game.Furniture.Floor;
using Skylight.API.Game.Inventory.Items.Floor;
using Skylight.API.Game.Users;

namespace Skylight.Server.Game.Inventory.Items.Floor;

internal sealed class SoundSetInventoryItem : FloorInventoryItem, ISoundSetInventoryItem
{
	public override ISoundSetFurniture Furniture { get; }

	public SoundSetInventoryItem(int id, IUserInfo owner, ISoundSetFurniture furniture)
		: base(id, owner)
	{
		this.Furniture = furniture;
	}
}
