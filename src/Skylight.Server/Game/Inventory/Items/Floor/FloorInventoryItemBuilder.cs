using Skylight.API.Game.Furniture;
using Skylight.API.Game.Inventory.Items.Floor;
using Skylight.Server.Game.Furniture;

namespace Skylight.Server.Game.Inventory.Items.Floor;

internal abstract class FloorInventoryItemBuilder<TFurniture, TTarget, TBuilder> : FurnitureItemBuilder<TFurniture, TTarget, TBuilder>, IFloorInventoryItemBuilder<TFurniture, TTarget, TBuilder>
	where TFurniture : IFurniture
	where TTarget : IFloorInventoryItem, IFurnitureItem<TFurniture>
	where TBuilder : FloorInventoryItemBuilder<TFurniture, TTarget, TBuilder>;
