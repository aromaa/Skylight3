using Skylight.API.Game.Furniture;
using Skylight.API.Game.Furniture.Wall;

namespace Skylight.API.Game.Inventory.Items.Wall.Data;

public interface IStickyNoteInventoryItemDataBuilder : IFurnitureItemDataBuilder<IStickyNoteFurniture, IStickyNoteInventoryItem, IStickyNoteInventoryItemDataBuilder>
{
	public IStickyNoteInventoryItemDataBuilder Count(int count);

	public new interface IRaw<out TBuilder> : IFurnitureItemDataBuilder.IRaw<TBuilder>, IStickyNoteInventoryItemDataBuilder
		where TBuilder : IRaw<TBuilder>;
}

public interface IStickyNoteInventoryItemDataBuilder<in TFurniture, out TTarget, out TBuilder> : IStickyNoteInventoryItemDataBuilder
	where TFurniture : IStickyNoteFurniture
	where TTarget : IStickyNoteInventoryItem, IFurnitureItem<TFurniture>
	where TBuilder : IStickyNoteInventoryItemDataBuilder<TFurniture, TTarget, TBuilder>, IFurnitureItemDataBuilder<TFurniture, TTarget, TBuilder>;

public interface IStickyNoteInventoryItemDataBuilder<in TFurniture, out TTarget, out TBuilder, out TFurnitureBuilder> : IStickyNoteInventoryItemDataBuilder<TFurniture, TTarget, TBuilder>, IWallInventoryItemBuilder<TFurniture, TTarget, TFurnitureBuilder, TBuilder>
	where TFurniture : IStickyNoteFurniture
	where TTarget : IStickyNoteInventoryItem, IFurnitureItem<TFurniture>
	where TBuilder : IStickyNoteInventoryItemDataBuilder<TFurniture, TTarget, TBuilder, TFurnitureBuilder>, IFurnitureItemDataBuilder<TFurniture, int, TTarget, TBuilder, TFurnitureBuilder>
	where TFurnitureBuilder : IWallInventoryItemBuilder<TFurniture, TTarget, TFurnitureBuilder, TBuilder>;
