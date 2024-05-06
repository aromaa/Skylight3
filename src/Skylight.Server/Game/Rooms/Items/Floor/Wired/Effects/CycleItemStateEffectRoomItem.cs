using System.Text.Json;
using Skylight.API.Game.Furniture.Floor.Wired.Effects;
using Skylight.API.Game.Rooms;
using Skylight.API.Game.Rooms.Items;
using Skylight.API.Game.Rooms.Items.Floor.Wired.Effects;
using Skylight.API.Game.Rooms.Items.Interactions.Wired.Effects;
using Skylight.API.Game.Rooms.Units;
using Skylight.API.Game.Users;
using Skylight.API.Numerics;
using Skylight.Protocol.Packets.Data.UserDefinedRoomEvents;
using Skylight.Protocol.Packets.Outgoing.UserDefinedRoomEvents;

namespace Skylight.Server.Game.Rooms.Items.Floor.Wired.Effects;

internal sealed class CycleItemStateEffectRoomItem(IRoom room, int id, IUserInfo owner, ICycleItemStateEffectFurniture furniture, Point3D position, int direction, IWiredEffectInteractionHandler interactionHandler,
	HashSet<IRoomItem>? selectedItems, JsonDocument? extraData)
	: WiredEffectRoomItem(room, id, owner, position, direction), ICycleItemStateRoomItem
{
	public override ICycleItemStateEffectFurniture Furniture { get; } = furniture;

	private readonly IWiredEffectInteractionHandler interactionHandler = interactionHandler;

	private LazyRoomItemSetHolder selectedItems = selectedItems is null
		? new LazyRoomItemSetHolder(WiredUtils.GetSelectedItems(extraData))
		: new LazyRoomItemSetHolder(selectedItems);

	public IReadOnlySet<IRoomItem> SelectedItems
	{
		get => this.selectedItems.Get(this.Room.ItemManager);
		set => this.selectedItems.Set([.. value]);
	}

	public override void OnPlace() => this.interactionHandler.OnPlace(this);
	public override void OnRemove() => this.interactionHandler.OnRemove(this);

	public override void Interact(IUserRoomUnit unit, int state)
	{
		unit.User.SendAsync(new WiredFurniActionOutgoingPacket(this.Id, this.Furniture.Id, ActionType.CycleItemState, 100, this.SelectedItems.Select(i => i.Id).ToList(), 0, [], string.Empty));
	}

	public override void Trigger(IUserRoomUnit? cause = null)
	{
		foreach (IRoomItem selectedItem in this.SelectedItems)
		{
			if (selectedItem is IInteractableRoomItem interactable)
			{
				interactable.Interact(null!, 0);
			}
		}
	}

	public JsonDocument GetExtraData()
	{
		return JsonSerializer.SerializeToDocument(new
		{
			SelectedItems = this.SelectedItems.Select(i => i.StripId)
		});
	}
}
