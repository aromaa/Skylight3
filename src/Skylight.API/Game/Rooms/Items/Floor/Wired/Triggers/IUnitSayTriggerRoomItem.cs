using Skylight.API.Game.Furniture;
using Skylight.API.Game.Furniture.Floor.Wired.Triggers;

namespace Skylight.API.Game.Rooms.Items.Floor.Wired.Triggers;

public interface IUnitSayTriggerRoomItem : IWiredTriggerRoomItem, IFurnitureItemData, IFurnitureItem<IUnitSayTriggerFurniture>
{
	public new IUnitSayTriggerFurniture Furniture { get; }

	public string Message { get; set; }
	public bool OwnerOnly { get; set; }

	IUnitSayTriggerFurniture IFurnitureItem<IUnitSayTriggerFurniture>.Furniture => this.Furniture;
}
