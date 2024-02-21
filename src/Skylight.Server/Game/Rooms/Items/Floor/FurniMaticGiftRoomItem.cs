using System.Text.Json;
using Skylight.API.Game.Furniture.Floor;
using Skylight.API.Game.Rooms;
using Skylight.API.Game.Rooms.Items.Floor;
using Skylight.API.Game.Users;
using Skylight.API.Numerics;

namespace Skylight.Server.Game.Rooms.Items.Floor;

internal sealed class FurniMaticGiftRoomItem(IRoom room, int id, IUserInfo owner, IFurniMaticGiftFurniture furniture, Point3D position, int direction, DateTime recycledAt)
	: FloorRoomItem(room, id, owner, position, direction), IFurniMaticGiftRoomItem
{
	public override IFurniMaticGiftFurniture Furniture { get; } = furniture;

	public DateTime RecycledAt { get; } = recycledAt;

	public override double Height => this.Furniture.DefaultHeight;

	public JsonDocument GetExtraData() => JsonSerializer.SerializeToDocument(this.RecycledAt);
}
