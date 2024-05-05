using System.Text.Json;
using Skylight.API.Game.Furniture.Floor.Wired.Triggers;
using Skylight.API.Game.Rooms;
using Skylight.API.Game.Rooms.Items.Floor.Wired.Triggers;
using Skylight.API.Game.Rooms.Items.Interactions.Wired.Triggers;
using Skylight.API.Game.Rooms.Units;
using Skylight.API.Game.Users;
using Skylight.API.Numerics;
using Skylight.Protocol.Packets.Data.UserDefinedRoomEvents;
using Skylight.Protocol.Packets.Outgoing.UserDefinedRoomEvents;

namespace Skylight.Server.Game.Rooms.Items.Floor.Wired.Triggers;

internal sealed class UnitSayTriggerRoomItem(IRoom room, int id, IUserInfo owner, IUnitSayTriggerFurniture furniture, Point3D position, int direction, IUnitSayTriggerInteractionHandler interactionHandler)
	: WiredTriggerRoomItem(room, id, owner, position, direction), IUnitSayTriggerRoomItem
{
	public override IUnitSayTriggerFurniture Furniture { get; } = furniture;

	private readonly IUnitSayTriggerInteractionHandler interactionHandler = interactionHandler;

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
		unit.User.SendAsync(new WiredFurniTriggerOutgoingPacket(this.Id, this.Furniture.Id, TriggerType.UnitSay, 0, [], [], this.Message));
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
