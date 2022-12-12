using System.Text.Json;
using Skylight.API.Game.Furniture.Floor;
using Skylight.API.Game.Users;

namespace Skylight.Server.Game.Inventory.Items.Floor.Factory;

internal sealed class SoundSetInventoryItemFactory : FurnitureInventoryItemFactory<ISoundSetFurniture, SoundSetInventoryItem>
{
	public override SoundSetInventoryItem Create(int itemId, IUserInfo owner, ISoundSetFurniture furniture, JsonDocument? extraData)
	{
		return new SoundSetInventoryItem(itemId, owner, furniture);
	}
}
