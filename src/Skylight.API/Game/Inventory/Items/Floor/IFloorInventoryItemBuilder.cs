using Skylight.API.Game.Furniture;

namespace Skylight.API.Game.Inventory.Items.Floor;

public interface IFloorInventoryItemBuilder;

public interface IFloorInventoryItemBuilder<out TTarget> : IFloorInventoryItemBuilder, IFurnitureInventoryItemBuilder<TTarget>
	where TTarget : IFloorInventoryItem;

public interface IFloorInventoryItemBuilder<in TFurniture, out TTarget> : IFloorInventoryItemBuilder<TTarget>, IFurnitureInventoryItemBuilder<TFurniture, TTarget>
	where TFurniture : IFurniture
	where TTarget : IFloorInventoryItem, IFurnitureItem<TFurniture>;

public interface IFloorInventoryItemBuilder<in TFurniture, out TTarget, out TBuilder> : IFloorInventoryItemBuilder<TFurniture, TTarget>, IFurnitureInventoryItemBuilder<TFurniture, TTarget, TBuilder>
	where TFurniture : IFurniture
	where TTarget : IFloorInventoryItem, IFurnitureItem<TFurniture>
	where TBuilder : IFurnitureInventoryItemBuilder<TFurniture, TTarget, TBuilder>;

public interface IFloorInventoryItemBuilder<in TFurniture, out TTarget, out TBuilder, out TDataBuilder> : IFloorInventoryItemBuilder<TFurniture, TTarget, TBuilder>, IFurnitureInventoryItemBuilder<TFurniture, TTarget, TBuilder, TDataBuilder>
	where TFurniture : IFurniture
	where TTarget : IFloorInventoryItem, IFurnitureItem<TFurniture>
	where TBuilder : IFurnitureInventoryItemBuilder<TFurniture, TTarget, TBuilder, TDataBuilder>
	where TDataBuilder : IFurnitureItemDataBuilder<TFurniture, TTarget, TDataBuilder, TBuilder>;
