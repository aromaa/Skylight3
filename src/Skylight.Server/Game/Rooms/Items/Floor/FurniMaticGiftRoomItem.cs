using System.Text.Json;
using Skylight.API.Game.Furniture.Floor;
using Skylight.API.Game.Rooms;
using Skylight.API.Game.Rooms.Items.Floor;
using Skylight.API.Game.Users;
using Skylight.API.Numerics;

namespace Skylight.Server.Game.Rooms.Items.Floor;

internal sealed class FurniMaticGiftRoomItem : FloorRoomItem, IFurniMaticGiftRoomItem
{
	public override IFurniMaticGiftFurniture Furniture { get; }

	public DateTime RecycledAt { get; }

	public FurniMaticGiftRoomItem(IRoom room, int id, IUserInfo owner, IFurniMaticGiftFurniture furniture, Point3D position, int direction, DateTime recycledAt)
		: base(room, id, owner, position, direction)
	{
		this.Furniture = furniture;

		this.RecycledAt = recycledAt;
	}

	public override double Height => this.Furniture.DefaultHeight;

	public JsonDocument GetExtraData() => JsonSerializer.SerializeToDocument(this.RecycledAt);
}
