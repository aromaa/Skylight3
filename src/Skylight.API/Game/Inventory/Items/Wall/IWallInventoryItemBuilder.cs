using Skylight.API.Game.Furniture;

namespace Skylight.API.Game.Inventory.Items.Wall;

public interface IWallInventoryItemBuilder;

public interface IWallInventoryItemBuilder<out TTarget> : IWallInventoryItemBuilder, IFurnitureInventoryItemBuilder<TTarget>
	where TTarget : IWallInventoryItem;

public interface IWallInventoryItemBuilder<in TFurniture, out TTarget> : IWallInventoryItemBuilder<TTarget>, IFurnitureInventoryItemBuilder<TFurniture, TTarget>
	where TFurniture : IFurniture
	where TTarget : IWallInventoryItem, IFurnitureItem<TFurniture>;

public interface IWallInventoryItemBuilder<in TFurniture, out TTarget, out TBuilder> : IWallInventoryItemBuilder<TFurniture, TTarget>, IFurnitureInventoryItemBuilder<TFurniture, TTarget, TBuilder>
	where TFurniture : IFurniture
	where TTarget : IWallInventoryItem, IFurnitureItem<TFurniture>
	where TBuilder : IFurnitureInventoryItemBuilder<TFurniture, TTarget, TBuilder>;

public interface IWallInventoryItemBuilder<in TFurniture, out TTarget, out TBuilder, out TDataBuilder> : IWallInventoryItemBuilder<TFurniture, TTarget, TBuilder>, IFurnitureInventoryItemBuilder<TFurniture, TTarget, TBuilder, TDataBuilder>
	where TFurniture : IFurniture
	where TTarget : IWallInventoryItem, IFurnitureItem<TFurniture>
	where TBuilder : IFurnitureInventoryItemBuilder<TFurniture, TTarget, TBuilder, TDataBuilder>
	where TDataBuilder : IFurnitureItemDataBuilder<TFurniture, TTarget, TDataBuilder, TBuilder>;
