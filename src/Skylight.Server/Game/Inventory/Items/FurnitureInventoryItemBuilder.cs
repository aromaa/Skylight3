using Skylight.API.Game.Furniture;
using Skylight.API.Game.Inventory.Items;
using Skylight.Server.Game.Furniture;

namespace Skylight.Server.Game.Inventory.Items;

internal abstract class FurnitureInventoryItemBuilder<TFurniture, TTarget, TBuilder> : FurnitureItemBuilder<TFurniture, int, TTarget, TBuilder>, IFurnitureInventoryItemBuilder<TFurniture, TTarget, TBuilder>
	where TFurniture : IFurniture
	where TTarget : IFurnitureInventoryItem, IFurnitureItem<TFurniture>
	where TBuilder : FurnitureInventoryItemBuilder<TFurniture, TTarget, TBuilder>
{
	protected override bool ValidId(int value) => value != 0;
}
