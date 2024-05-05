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

internal sealed class UnitEnterRoomTriggerRoomItem(IRoom room, int id, IUserInfo owner, IUnitEnterRoomTriggerFurniture furniture, Point3D position, int direction, IUnitEnterRoomTriggerInteractionHandler interactionHandler)
	: WiredTriggerRoomItem(room, id, owner, position, direction), IUnitEnterRoomTriggerRoomItem
{
	public override IUnitEnterRoomTriggerFurniture Furniture { get; } = furniture;

	private readonly IUnitEnterRoomTriggerInteractionHandler interactionHandler = interactionHandler;

	public string? TriggerUsername { get; set; }

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
		unit.User.SendAsync(new WiredFurniTriggerOutgoingPacket(this.Id, this.Furniture.Id, TriggerType.UnitEnterRoom, 0, [], [], this.TriggerUsername ?? string.Empty));
	}

	public JsonDocument GetExtraData()
	{
		return JsonSerializer.SerializeToDocument(new
		{
			this.TriggerUsername
		});
	}
}
