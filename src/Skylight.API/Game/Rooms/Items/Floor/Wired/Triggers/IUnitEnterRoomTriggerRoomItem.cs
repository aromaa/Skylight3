using Skylight.API.Game.Furniture;
using Skylight.API.Game.Furniture.Floor.Wired.Triggers;

namespace Skylight.API.Game.Rooms.Items.Floor.Wired.Triggers;

public interface IUnitEnterRoomTriggerRoomItem : IWiredTriggerRoomItem, IFurnitureItemData, IFurnitureItem<IUnitEnterRoomTriggerFurniture>
{
	public new IUnitEnterRoomTriggerFurniture Furniture { get; }

	public string? TriggerUsername { get; set; }

	IUnitEnterRoomTriggerFurniture IFurnitureItem<IUnitEnterRoomTriggerFurniture>.Furniture => this.Furniture;
}
