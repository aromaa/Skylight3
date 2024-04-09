using Skylight.API.Game.Furniture;
using Skylight.API.Game.Furniture.Floor.Wired.Triggers;

namespace Skylight.API.Game.Rooms.Items.Floor.Wired.Triggers;

public interface IUserSayTriggerRoomItem : IWiredTriggerRoomItem, IFurnitureItemData, IFurnitureItem<IUserSayTriggerFurniture>
{
	public new IUserSayTriggerFurniture Furniture { get; }

	public string Message { get; set; }
	public bool OwnerOnly { get; set; }

	IUserSayTriggerFurniture IFurnitureItem<IUserSayTriggerFurniture>.Furniture => this.Furniture;
}
