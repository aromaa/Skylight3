using Skylight.API.Game.Furniture;
using Skylight.API.Game.Furniture.Floor.Wired.Effects;

namespace Skylight.API.Game.Rooms.Items.Floor.Wired.Effects;

public interface ITeleportUnitEffectRoomItem : IWiredEffectRoomItem, IFurnitureItemData, IFurnitureItem<ITeleportUnitEffectFurniture>
{
	public new ITeleportUnitEffectFurniture Furniture { get; }

	public IReadOnlySet<IRoomItem> SelectedItems { get; set; }

	ITeleportUnitEffectFurniture IFurnitureItem<ITeleportUnitEffectFurniture>.Furniture => this.Furniture;
}
