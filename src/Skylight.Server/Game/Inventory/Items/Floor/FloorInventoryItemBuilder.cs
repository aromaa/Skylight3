using Skylight.API.Game.Furniture;
using Skylight.API.Game.Inventory.Items.Floor;

namespace Skylight.Server.Game.Inventory.Items.Floor;

internal abstract class FloorInventoryItemBuilder<TFurniture, TTarget, TBuilder> : FurnitureInventoryItemBuilder<TFurniture, TTarget, TBuilder>, IFloorInventoryItemBuilder<TFurniture, TTarget, TBuilder>
	where TFurniture : IFurniture
	where TTarget : IFloorInventoryItem, IFurnitureItem<TFurniture>
	where TBuilder : FloorInventoryItemBuilder<TFurniture, TTarget, TBuilder>;
