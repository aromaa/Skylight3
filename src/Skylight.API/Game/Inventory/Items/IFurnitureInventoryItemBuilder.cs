using Skylight.API.Game.Furniture;

namespace Skylight.API.Game.Inventory.Items;

public interface IFurnitureInventoryItemBuilder : IFurnitureItemBuilder;

public interface IFurnitureInventoryItemBuilder<out TTarget> : IFurnitureInventoryItemBuilder, IFurnitureItemBuilder<int, TTarget>
	where TTarget : IFurnitureInventoryItem;

public interface IFurnitureInventoryItemBuilder<in TFurniture, out TTarget> : IFurnitureInventoryItemBuilder<TTarget>, IFurnitureItemBuilder<TFurniture, int, TTarget>
	where TFurniture : IFurniture
	where TTarget : IFurnitureInventoryItem, IFurnitureItem<TFurniture>;

public interface IFurnitureInventoryItemBuilder<in TFurniture, out TTarget, out TBuilder> : IFurnitureInventoryItemBuilder<TFurniture, TTarget>, IFurnitureItemBuilder<TFurniture, int, TTarget, TBuilder>
	where TFurniture : IFurniture
	where TTarget : IFurnitureInventoryItem, IFurnitureItem<TFurniture>
	where TBuilder : IFurnitureInventoryItemBuilder<TFurniture, TTarget, TBuilder>;

public interface IFurnitureInventoryItemBuilder<in TFurniture, out TTarget, out TBuilder, out TDataBuilder> : IFurnitureInventoryItemBuilder<TFurniture, TTarget, TBuilder>, IFurnitureItemBuilder<TFurniture, int, TTarget, TBuilder, TDataBuilder>
	where TFurniture : IFurniture
	where TTarget : IFurnitureInventoryItem, IFurnitureItem<TFurniture>
	where TBuilder : IFurnitureInventoryItemBuilder<TFurniture, TTarget, TBuilder, TDataBuilder>
	where TDataBuilder : IFurnitureItemDataBuilder<TFurniture, int, TTarget, TDataBuilder, TBuilder>;
