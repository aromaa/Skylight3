using Skylight.API.Game.Furniture;
using Skylight.API.Game.Furniture.Floor.Wired;
using Skylight.API.Game.Furniture.Floor.Wired.Effects;
using Skylight.API.Game.Rooms.Units;

namespace Skylight.API.Game.Rooms.Items.Floor.Wired.Effects;

public interface IWiredEffectRoomItem : IWiredRoomItem, IFurnitureItem<IWiredEffectFurniture>
{
	public new IWiredEffectFurniture Furniture { get; }

	public int EffectDelay { get; set; }

	public void Trigger(IUserRoomUnit? cause = null);

	IWiredFurniture IWiredRoomItem.Furniture => this.Furniture;
	IWiredEffectFurniture IFurnitureItem<IWiredEffectFurniture>.Furniture => this.Furniture;
}
