using System.Text.Json;
using Skylight.API.Game.Furniture.Floor;
using Skylight.API.Game.Inventory.Items.Floor;
using Skylight.API.Game.Users;

namespace Skylight.Server.Game.Inventory.Items.Floor.Factory;

internal sealed class FurniMaticGiftInventoryItemFactory : FurnitureInventoryItemFactory<IFurniMaticGiftFurniture, IFurniMaticGiftInventoryItem, DateTime>
{
	public override IFurniMaticGiftInventoryItem Create(int itemId, IUserInfo owner, IFurniMaticGiftFurniture furniture, DateTime extraData)
	{
		return new FurniMaticGiftInventoryItem(itemId, owner, furniture, extraData);
	}

	public override IFurniMaticGiftInventoryItem Create(int itemId, IUserInfo owner, IFurniMaticGiftFurniture furniture, JsonDocument? extraData)
	{
		return new FurniMaticGiftInventoryItem(itemId, owner, furniture, extraData?.RootElement.GetDateTime() ?? DateTime.UnixEpoch);
	}
}
