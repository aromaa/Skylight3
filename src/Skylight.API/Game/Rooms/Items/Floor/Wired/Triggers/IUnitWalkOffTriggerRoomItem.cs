using Skylight.API.Game.Furniture;
using Skylight.API.Game.Furniture.Floor.Wired.Triggers;

namespace Skylight.API.Game.Rooms.Items.Floor.Wired.Triggers;

public interface IUnitWalkOffTriggerRoomItem : IWiredTriggerRoomItem, IFurnitureItemData, IFurnitureItem<IUnitWalkOffTriggerFurniture>
{
	public new IUnitWalkOffTriggerFurniture Furniture { get; }

	public IReadOnlySet<IRoomItem> SelectedItems { get; set; }

	IUnitWalkOffTriggerFurniture IFurnitureItem<IUnitWalkOffTriggerFurniture>.Furniture => this.Furniture;
}
