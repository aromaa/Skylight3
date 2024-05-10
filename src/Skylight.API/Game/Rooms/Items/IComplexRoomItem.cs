using Skylight.API.Game.Furniture;

namespace Skylight.API.Game.Rooms.Items;

public interface IComplexRoomItem : IRoomItem, IFurnitureItem<IComplexFurniture>
{
	public new IComplexFurniture Furniture { get; }

	IComplexFurniture IFurnitureItem<IComplexFurniture>.Furniture => this.Furniture;
}
