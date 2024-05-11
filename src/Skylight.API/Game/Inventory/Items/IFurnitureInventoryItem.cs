using Skylight.API.Game.Furniture;

namespace Skylight.API.Game.Inventory.Items;

public interface IFurnitureInventoryItem : IInventoryItem, IFurnitureItem<IFurniture>
{
	public int Id { get; }

	public int StripId { get; }
}
