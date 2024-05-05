using Skylight.API.Game.Furniture;
using Skylight.API.Game.Furniture.Floor.Wired.Triggers;

namespace Skylight.API.Game.Rooms.Items.Floor.Wired.Triggers;

public interface IUnitUseItemTriggerRoomItem : IWiredTriggerRoomItem, IFurnitureItemData, IFurnitureItem<IUnitUseItemTriggerFurniture>
{
	public new IUnitUseItemTriggerFurniture Furniture { get; }

	public IReadOnlySet<IRoomItem> SelectedItems { get; set; }

	IUnitUseItemTriggerFurniture IFurnitureItem<IUnitUseItemTriggerFurniture>.Furniture => this.Furniture;
}
