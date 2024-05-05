using System.Text.Json;
using Skylight.API.Game.Furniture.Floor.Wired.Triggers;
using Skylight.API.Game.Rooms;
using Skylight.API.Game.Rooms.Items;
using Skylight.API.Game.Rooms.Items.Floor.Wired.Triggers;
using Skylight.API.Game.Rooms.Items.Interactions.Wired.Triggers;
using Skylight.API.Game.Rooms.Units;
using Skylight.API.Game.Users;
using Skylight.API.Numerics;
using Skylight.Protocol.Packets.Data.UserDefinedRoomEvents;
using Skylight.Protocol.Packets.Outgoing.UserDefinedRoomEvents;

namespace Skylight.Server.Game.Rooms.Items.Floor.Wired.Triggers;

internal sealed class UnitUseItemTriggerRoomItem(IRoom room, int id, IUserInfo owner, IUnitUseItemTriggerFurniture furniture, Point3D position, int direction, IUnitUseItemTriggerInteractionHandler interactionHandler)
	: WiredTriggerRoomItem(room, id, owner, position, direction), IUnitUseItemTriggerRoomItem
{
	public override IUnitUseItemTriggerFurniture Furniture { get; } = furniture;

	private readonly IUnitUseItemTriggerInteractionHandler interactionHandler = interactionHandler;

	public IReadOnlySet<IRoomItem> SelectedItems { get; set; } = new HashSet<IRoomItem>();

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
		unit.User.SendAsync(new WiredFurniTriggerOutgoingPacket(this.Id, this.Furniture.Id, TriggerType.UnitUseItem, 100, this.SelectedItems.Select(i => i.Id).ToList(), [], string.Empty));
	}

	public JsonDocument GetExtraData()
	{
		return JsonSerializer.SerializeToDocument(new
		{
			SelectedItems = this.SelectedItems.Select(i => i.StripId)
		});
	}
}
