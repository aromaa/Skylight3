using System.Text.Json;
using Skylight.API.Game.Furniture.Floor.Wired.Triggers;
using Skylight.API.Game.Rooms.Items.Floor.Wired.Triggers;
using Skylight.API.Game.Rooms.Items.Interactions.Wired.Triggers;
using Skylight.API.Game.Rooms.Private;
using Skylight.API.Game.Rooms.Units;
using Skylight.API.Game.Users;
using Skylight.API.Numerics;
using Skylight.Protocol.Packets.Data.UserDefinedRoomEvents;
using Skylight.Protocol.Packets.Outgoing.UserDefinedRoomEvents;

namespace Skylight.Server.Game.Rooms.Items.Floor.Wired.Triggers;

internal sealed class UnitSayTriggerRoomItem(IPrivateRoom room, int id, IUserInfo owner, IUnitSayTriggerFurniture furniture, Point3D position, int direction, IUnitSayTriggerInteractionHandler interactionHandler,
	string message)
	: WiredTriggerRoomItem<IUnitSayTriggerFurniture>(room, id, owner, furniture, position, direction), IUnitSayTriggerRoomItem
{
	private readonly IUnitSayTriggerInteractionHandler interactionHandler = interactionHandler;

	public new IUnitSayTriggerFurniture Furniture => this.furniture;

	public string Message { get; set; } = message;
	public bool OwnerOnly { get; set; }

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
