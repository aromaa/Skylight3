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

internal sealed class UnitUseItemTriggerRoomItem(IRoom room, int id, IUserInfo owner, IUnitUseItemTriggerFurniture furniture, Point3D position, int direction, IUnitUseItemTriggerInteractionHandler interactionHandler,
	HashSet<IRoomItem>? selectedItems, JsonDocument? extraData)
	: WiredTriggerRoomItem<IUnitUseItemTriggerFurniture>(room, id, owner, furniture, position, direction), IUnitUseItemTriggerRoomItem
{
	private readonly IUnitUseItemTriggerInteractionHandler interactionHandler = interactionHandler;

	private LazyRoomItemSetHolder selectedItems = selectedItems is null
		? new LazyRoomItemSetHolder(WiredUtils.GetSelectedItems(extraData))
		: new LazyRoomItemSetHolder(selectedItems);

	public new IUnitUseItemTriggerFurniture Furniture => this.furniture;

	public IReadOnlySet<IRoomItem> SelectedItems
	{
		get => this.selectedItems.Get(this.Room.ItemManager);
		set => this.selectedItems.Set([.. value]);
	}

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
