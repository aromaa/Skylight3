using Skylight.API.Game.Furniture;
using Skylight.API.Game.Furniture.Floor;

namespace Skylight.API.Game.Inventory.Items.Floor.Data;

public interface IFurniMaticGiftInventoryItemDataBuilder : IFurnitureItemDataBuilder<IFurniMaticGiftFurniture, IFurniMaticGiftInventoryItem, IFurniMaticGiftInventoryItemDataBuilder>
{
	public IFurniMaticGiftInventoryItemDataBuilder Recycled(DateTime recycledAt);

	public new interface IRaw<out TBuilder> : IFurnitureItemDataBuilder.IRaw<TBuilder>, IFurniMaticGiftInventoryItemDataBuilder
		where TBuilder : IRaw<TBuilder>;
}

public interface IFurniMaticGiftInventoryItemDataBuilder<in TFurniture, out TTarget, out TBuilder> : IFurniMaticGiftInventoryItemDataBuilder
	where TFurniture : IFurniMaticGiftFurniture
	where TTarget : IFurniMaticGiftInventoryItem, IFurnitureItem<TFurniture>
	where TBuilder : IFurniMaticGiftInventoryItemDataBuilder<TFurniture, TTarget, TBuilder>, IFurnitureItemDataBuilder<TFurniture, TTarget, TBuilder>;

public interface IFurniMaticGiftInventoryItemDataBuilder<in TFurniture, out TTarget, out TBuilder, out TFurnitureBuilder> : IFurniMaticGiftInventoryItemDataBuilder<TFurniture, TTarget, TBuilder>, IFloorInventoryItemBuilder<TFurniture, TTarget, TFurnitureBuilder, TBuilder>
	where TFurniture : IFurniMaticGiftFurniture
	where TTarget : IFurniMaticGiftInventoryItem, IFurnitureItem<TFurniture>
	where TBuilder : IFurniMaticGiftInventoryItemDataBuilder<TFurniture, TTarget, TBuilder, TFurnitureBuilder>, IFurnitureItemDataBuilder<TFurniture, int, TTarget, TBuilder, TFurnitureBuilder>
	where TFurnitureBuilder : IFloorInventoryItemBuilder<TFurniture, TTarget, TFurnitureBuilder, TBuilder>;
