using System.Text.Json;
using Skylight.API.Game.Furniture.Floor;
using Skylight.API.Game.Inventory.Items.Floor;
using Skylight.API.Game.Users;

namespace Skylight.Server.Game.Inventory.Items.Floor;

internal sealed class FurniMaticGiftInventoryItem(int id, IUserInfo owner, IFurniMaticGiftFurniture furniture, DateTime recycledAt)
	: FloorInventoryItem(id, owner), IFurniMaticGiftInventoryItem
{
	public override IFurniMaticGiftFurniture Furniture { get; } = furniture;

	public DateTime RecycledAt { get; } = recycledAt;

	public JsonDocument GetExtraData() => JsonSerializer.SerializeToDocument(this.RecycledAt);
}
