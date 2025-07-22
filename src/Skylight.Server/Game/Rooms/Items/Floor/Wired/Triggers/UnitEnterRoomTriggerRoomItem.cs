using System.Text.Json;
using Skylight.API.Game.Furniture.Floor.Wired.Triggers;
using Skylight.API.Game.Rooms.Items;
using Skylight.API.Game.Rooms.Items.Floor.Wired.Triggers;
using Skylight.API.Game.Rooms.Items.Interactions.Wired.Triggers;
using Skylight.API.Game.Rooms.Private;
using Skylight.API.Game.Rooms.Units;
using Skylight.API.Game.Users;
using Skylight.API.Numerics;
using Skylight.Protocol.Packets.Data.UserDefinedRoomEvents;
using Skylight.Protocol.Packets.Outgoing.UserDefinedRoomEvents;

namespace Skylight.Server.Game.Rooms.Items.Floor.Wired.Triggers;

internal sealed class UnitEnterRoomTriggerRoomItem(IPrivateRoom room, RoomItemId id, IUserInfo owner, IUnitEnterRoomTriggerFurniture furniture, Point3D position, int direction, IUnitEnterRoomTriggerInteractionHandler interactionHandler,
	string triggerUsername)
	: WiredTriggerRoomItem<IUnitEnterRoomTriggerFurniture>(room, id, owner, furniture, position, direction), IUnitEnterRoomTriggerRoomItem
{
	private readonly IUnitEnterRoomTriggerInteractionHandler interactionHandler = interactionHandler;

	public new IUnitEnterRoomTriggerFurniture Furniture => this.furniture;

	public string? TriggerUsername { get; set; } = triggerUsername;

	public override void OnPlace()
	{
		this.interactionHandler.OnPlace(this);
	}

	public override void OnRemove()
	{
		this.interactionHandler.OnRemove(this);
	}

	public override void Open(IUserRoomUnit unit)
	{
		unit.User.SendAsync(new WiredFurniTriggerOutgoingPacket<RoomItemId>(this.Id, this.Furniture.Id, TriggerType.UnitEnterRoom, 0, [], [], this.TriggerUsername ?? string.Empty));
	}

	public JsonDocument GetExtraData()
	{
		return JsonSerializer.SerializeToDocument(new
		{
			this.TriggerUsername
		});
	}
}
