using Skylight.API.Game.Furniture;
using Skylight.API.Game.Furniture.Floor.Wired.Triggers;

namespace Skylight.API.Game.Rooms.Items.Floor.Wired.Triggers;

public interface IUnitWalkOnTriggerRoomItem : IWiredTriggerRoomItem, IFurnitureItemData, IFurnitureItem<IUnitWalkOnTriggerFurniture>
{
	public new IUnitWalkOnTriggerFurniture Furniture { get; }

	public IReadOnlySet<IRoomItem> SelectedItems { get; set; }

	IUnitWalkOnTriggerFurniture IFurnitureItem<IUnitWalkOnTriggerFurniture>.Furniture => this.Furniture;
}
