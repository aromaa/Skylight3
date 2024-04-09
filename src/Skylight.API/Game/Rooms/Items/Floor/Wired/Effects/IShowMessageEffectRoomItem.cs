using Skylight.API.Game.Furniture;
using Skylight.API.Game.Furniture.Floor.Wired.Effects;

namespace Skylight.API.Game.Rooms.Items.Floor.Wired.Effects;

public interface IShowMessageEffectRoomItem : IWiredEffectRoomItem, IFurnitureItemData, IFurnitureItem<IShowMessageEffectFurniture>
{
	public new IShowMessageEffectFurniture Furniture { get; }

	public string Message { get; set; }

	IShowMessageEffectFurniture IFurnitureItem<IShowMessageEffectFurniture>.Furniture => this.Furniture;
}
