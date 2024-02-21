using Skylight.API.Game.Furniture;
using Skylight.API.Game.Furniture.Wall;

namespace Skylight.API.Game.Inventory.Items.Wall;

public interface IWallInventoryItem : IFurnitureInventoryItem, IFurnitureItem<IWallFurniture>
{
	public new IWallFurniture Furniture { get; }

	IFurniture IFurnitureItem<IFurniture>.Furniture => this.Furniture;
	IWallFurniture IFurnitureItem<IWallFurniture>.Furniture => this.Furniture;
}

public interface IWallInventoryItem<out T> : IWallInventoryItem, IFurnitureInventoryItem<T>
	where T : IWallFurniture
{
	public new T Furniture { get; }

	IFurniture IFurnitureItem<IFurniture>.Furniture => this.Furniture;
}
