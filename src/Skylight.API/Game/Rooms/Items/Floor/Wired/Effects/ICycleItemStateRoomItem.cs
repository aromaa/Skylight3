using Skylight.API.Game.Furniture.Floor.Wired.Effects;
using Skylight.API.Game.Furniture;

namespace Skylight.API.Game.Rooms.Items.Floor.Wired.Effects;

public interface ICycleItemStateRoomItem : IWiredEffectRoomItem, IFurnitureItemData, IFurnitureItem<ICycleItemStateEffectFurniture>
{
	public new ICycleItemStateEffectFurniture Furniture { get; }

	public IReadOnlySet<IRoomItem> SelectedItems { get; set; }

	ICycleItemStateEffectFurniture IFurnitureItem<ICycleItemStateEffectFurniture>.Furniture => this.Furniture;
}
