using System.Text.Json;
using Skylight.API.Game.Furniture.Floor.Wired.Triggers;
using Skylight.API.Game.Rooms;
using Skylight.API.Game.Rooms.Items.Floor.Wired.Triggers;
using Skylight.API.Game.Rooms.Items.Interactions.Wired.Triggers;
using Skylight.API.Game.Rooms.Units;
using Skylight.API.Game.Users;
using Skylight.API.Numerics;
using Skylight.Protocol.Packets.Outgoing.UserDefinedRoomEvents;

namespace Skylight.Server.Game.Rooms.Items.Floor.Wired.Triggers;

internal sealed class UserSayTriggerRoomItem(IRoom room, int id, IUserInfo owner, IUserSayTriggerFurniture furniture, Point3D position, int direction, IUserSayTriggerInteractionHandler interactionHandler)
	: WiredTriggerRoomItem(room, id, owner, position, direction), IUserSayTriggerRoomItem
{
	public override IUserSayTriggerFurniture Furniture { get; } = furniture;

	private readonly IUserSayTriggerInteractionHandler interactionHandler = interactionHandler;

	public required string Message { get; set; }
	public bool OwnerOnly { get; set; }

	public override void OnPlace()
	{
		this.interactionHandler.OnPlace(this);
	}

	public override void OnRemove()
	{
		this.interactionHandler.OnRemove(this);
	}

	public override void Interact(IUserRoomUnit unit, int state)
	{
		unit.User.SendAsync(new WiredFurniTriggerOutgoingPacket(this.Id, this.Message));
	}

	public JsonDocument GetExtraData()
	{
		return JsonSerializer.SerializeToDocument(new
		{
			this.Message,
			this.OwnerOnly
		});
	}
}
