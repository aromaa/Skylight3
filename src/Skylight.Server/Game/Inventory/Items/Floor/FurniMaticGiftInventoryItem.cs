using System.Text.Json;
using Skylight.API.Game.Furniture.Floor;
using Skylight.API.Game.Inventory.Items.Floor;
using Skylight.API.Game.Users;

namespace Skylight.Server.Game.Inventory.Items.Floor;

internal sealed class FurniMaticGiftInventoryItem : FloorInventoryItem, IFurniMaticGiftInventoryItem
{
	public override IFurniMaticGiftFurniture Furniture { get; }

	public DateTime RecycledAt { get; }

	public FurniMaticGiftInventoryItem(int id, IUserInfo owner, IFurniMaticGiftFurniture furniture, DateTime recucledAt)
		: base(id, owner)
	{
		this.Furniture = furniture;

		this.RecycledAt = recucledAt;
	}

	public JsonDocument AsExtraData() => JsonSerializer.SerializeToDocument(this.RecycledAt);
}
