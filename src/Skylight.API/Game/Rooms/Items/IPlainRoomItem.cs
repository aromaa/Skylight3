using Skylight.API.Game.Furniture;

namespace Skylight.API.Game.Rooms.Items;

public interface IPlainRoomItem : IRoomItem, IFurnitureItem<IPlainFurniture>
{
	public new IPlainFurniture Furniture { get; }

	IPlainFurniture IFurnitureItem<IPlainFurniture>.Furniture => this.Furniture;
}
