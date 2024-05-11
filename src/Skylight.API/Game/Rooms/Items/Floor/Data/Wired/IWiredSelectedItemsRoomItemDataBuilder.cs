using Skylight.API.Game.Furniture;
using Skylight.API.Game.Furniture.Floor;
using Skylight.API.Game.Furniture.Floor.Wired;
using Skylight.API.Game.Inventory.Items.Floor;
using Skylight.API.Game.Rooms.Items.Floor.Wired;

namespace Skylight.API.Game.Rooms.Items.Floor.Data.Wired;

public interface IWiredSelectedItemsRoomItemDataBuilder : IFurnitureItemDataBuilder<IFurniMaticGiftFurniture, IFurniMaticGiftInventoryItem, IWiredSelectedItemsRoomItemDataBuilder>
{
	public IWiredSelectedItemsRoomItemDataBuilder SelectedItems(List<IFurniture> selectedItems);

	public new interface IRaw<out TBuilder> : IFurnitureItemDataBuilder.IRaw<TBuilder>, IWiredSelectedItemsRoomItemDataBuilder
		where TBuilder : IWiredSelectedItemsRoomItemDataBuilder.IRaw<TBuilder>;
}

public interface IWiredSelectedItemsRoomItemDataBuilder<in TFurniture, out TTarget, out TBuilder> : IWiredSelectedItemsRoomItemDataBuilder
	where TFurniture : IWiredFurniture
	where TTarget : IWiredRoomItem, IFurnitureItem<TFurniture>
	where TBuilder : IWiredSelectedItemsRoomItemDataBuilder<TFurniture, TTarget, TBuilder>, IFurnitureItemDataBuilder<TFurniture, TTarget, TBuilder>;

public interface IWiredSelectedItemsRoomItemDataBuilder<in TFurniture, out TTarget, out TBuilder, out TFurnitureBuilder> : IWiredSelectedItemsRoomItemDataBuilder<TFurniture, TTarget, TBuilder>, IFloorRoomItemBuilder<TFurniture, TTarget, TFurnitureBuilder, TBuilder>
	where TFurniture : IWiredFurniture
	where TTarget : IWiredRoomItem, IFurnitureItem<TFurniture>
	where TBuilder : IWiredSelectedItemsRoomItemDataBuilder<TFurniture, TTarget, TBuilder, TFurnitureBuilder>, IFurnitureItemDataBuilder<TFurniture, TTarget, TBuilder, TFurnitureBuilder>
	where TFurnitureBuilder : IFloorRoomItemBuilder<TFurniture, TTarget, TFurnitureBuilder, TBuilder>;
