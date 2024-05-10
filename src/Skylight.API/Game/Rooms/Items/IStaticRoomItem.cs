using Skylight.API.Game.Furniture;

namespace Skylight.API.Game.Rooms.Items;

public interface IStaticRoomItem : IPlainRoomItem, IFurnitureItem<IStaticFurniture>
{
	public new IStaticFurniture Furniture { get; }

	IStaticFurniture IFurnitureItem<IStaticFurniture>.Furniture => this.Furniture;
}
