using Skylight.API.Game.Furniture;
using Skylight.API.Game.Furniture.Floor.Wired;
using Skylight.API.Game.Furniture.Floor.Wired.Triggers;

namespace Skylight.API.Game.Rooms.Items.Floor.Wired.Triggers;

public interface IWiredTriggerRoomItem : IWiredRoomItem, IFurnitureItem<IWiredTriggerFurniture>
{
	public new IWiredTriggerFurniture Furniture { get; }

	IWiredFurniture IWiredRoomItem.Furniture => this.Furniture;
	IWiredTriggerFurniture IFurnitureItem<IWiredTriggerFurniture>.Furniture => this.Furniture;
}
