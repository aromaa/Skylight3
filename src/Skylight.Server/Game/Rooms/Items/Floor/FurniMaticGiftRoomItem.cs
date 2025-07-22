using System.Text.Json;
using Skylight.API.Game.Furniture.Floor;
using Skylight.API.Game.Rooms.Items;
using Skylight.API.Game.Rooms.Items.Floor;
using Skylight.API.Game.Rooms.Private;
using Skylight.API.Game.Users;
using Skylight.API.Numerics;

namespace Skylight.Server.Game.Rooms.Items.Floor;

internal sealed class FurniMaticGiftRoomItem(IPrivateRoom room, RoomItemId id, IUserInfo owner, IFurniMaticGiftFurniture furniture, Point3D position, int direction, DateTime recycledAt)
	: PlainFloorRoomItem<IFurniMaticGiftFurniture>(room, id, owner, furniture, position, direction), IFurniMaticGiftRoomItem
{
	public DateTime RecycledAt { get; } = recycledAt;

	public new IFurniMaticGiftFurniture Furniture => this.furniture;

	public bool CanOpen(IUser user) => this.Owner.Id == user.Profile.Id;

	public JsonDocument GetExtraData() => JsonSerializer.SerializeToDocument(this.RecycledAt);
}
