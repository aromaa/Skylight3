using Skylight.API.Game.Furniture;
using Skylight.API.Game.Furniture.Wall;

namespace Skylight.API.Game.Inventory.Items;

public interface IFurnitureInventoryItem : IInventoryItem, IFurnitureItem<IFurniture>
{
	public int Id { get; }

	public int StripId { get; }
}

public interface IFurnitureInventoryItem<out T> : IFurnitureInventoryItem, IFurnitureItem<IWallFurniture>
	where T : IFurniture
{
	public new T Furniture { get; }

	IFurniture IFurnitureItem<IFurniture>.Furniture => this.Furniture;
}
