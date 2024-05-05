using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Skylight.API.Game.Furniture.Floor;
using Skylight.API.Game.Furniture.Floor.Wired.Triggers;
using Skylight.API.Game.Rooms.Items;
using Skylight.API.Game.Rooms.Items.Floor;
using Skylight.API.Game.Rooms.Items.Floor.Builders;
using Skylight.API.Game.Rooms.Items.Interactions.Wired.Triggers;

namespace Skylight.Server.Game.Rooms.Items.Floor.Wired.Triggers.Builders;

internal sealed class UnitUseItemTriggerRoomItemBuilderImpl : FloorRoomItemBuilder
{
	private IUnitUseItemTriggerFurniture? FurnitureValue { get; set; }

	private HashSet<int>? SelectedItemsValue { get; set; }

	public override FloorRoomItemBuilder Furniture(IFloorFurniture furniture)
	{
		this.FurnitureValue = (IUnitUseItemTriggerFurniture)furniture;

		return this;
	}

	public override FloorRoomItemBuilder ExtraData(JsonDocument extraData)
	{
		if (extraData.RootElement.TryGetProperty("SelectedItems", out JsonElement selectedItemsValue))
		{
			this.SelectedItemsValue = [];
			foreach (JsonElement selectedItemValue in selectedItemsValue.EnumerateArray())
			{
				this.SelectedItemsValue.Add(selectedItemValue.GetInt32());
			}
		}

		return this;
	}

	public override IFloorRoomItem Build()
	{
		this.CheckValid();

		if (!this.RoomValue.ItemManager.TryGetInteractionHandler(out IUnitUseItemTriggerInteractionHandler? handler))
		{
			throw new Exception($"{typeof(IUnitUseItemTriggerInteractionHandler)} not found");
		}

		HashSet<IRoomItem> selectedItems = [];
		foreach (int stripId in this.SelectedItemsValue ?? [])
		{
			if (this.RoomValue.ItemManager.TryGetItem(stripId, out IRoomItem? item))
			{
				selectedItems.Add(item);
			}
		}

		return new UnitUseItemTriggerRoomItem(this.RoomValue, this.ItemIdValue, this.OwnerValue, this.FurnitureValue, this.PositionValue, this.DirectionValue, handler)
		{
			SelectedItems = selectedItems
		};
	}

	[MemberNotNull(nameof(this.FurnitureValue))]
	protected override void CheckValid()
	{
		base.CheckValid();

		ArgumentNullException.ThrowIfNull(this.FurnitureValue);
	}
}
