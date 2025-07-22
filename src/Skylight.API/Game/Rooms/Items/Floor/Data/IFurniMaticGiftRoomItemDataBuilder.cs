using Skylight.API.Game.Furniture;
using Skylight.API.Game.Furniture.Floor;

namespace Skylight.API.Game.Rooms.Items.Floor.Data;

public interface IFurniMaticGiftRoomItemDataBuilder : IFurnitureItemDataBuilder<IFurniMaticGiftFurniture, IFurniMaticGiftRoomItem, IFurniMaticGiftRoomItemDataBuilder>
{
	public IFurniMaticGiftRoomItemDataBuilder Recycled(DateTime recycledAt);

	public new interface IRaw<out TBuilder> : IFurnitureItemDataBuilder.IRaw<TBuilder>, IFurniMaticGiftRoomItemDataBuilder
		where TBuilder : IRaw<TBuilder>;
}

public interface IFurniMaticGiftRoomItemDataBuilder<in TFurniture, out TTarget, out TBuilder> : IFurniMaticGiftRoomItemDataBuilder
	where TFurniture : IFurniMaticGiftFurniture
	where TTarget : IFurniMaticGiftRoomItem, IFurnitureItem<TFurniture>
	where TBuilder : IFurniMaticGiftRoomItemDataBuilder<TFurniture, TTarget, TBuilder>, IFurnitureItemDataBuilder<TFurniture, TTarget, TBuilder>;

public interface IFurniMaticGiftRoomItemDataBuilder<in TFurniture, out TTarget, out TBuilder, out TFurnitureBuilder> : IFurniMaticGiftRoomItemDataBuilder<TFurniture, TTarget, TBuilder>, IFloorRoomItemBuilder<TFurniture, TTarget, TFurnitureBuilder, TBuilder>
	where TFurniture : IFurniMaticGiftFurniture
	where TTarget : IFurniMaticGiftRoomItem, IFurnitureItem<TFurniture>
	where TBuilder : IFurniMaticGiftRoomItemDataBuilder<TFurniture, TTarget, TBuilder, TFurnitureBuilder>, IFurnitureItemDataBuilder<TFurniture, RoomItemId, TTarget, TBuilder, TFurnitureBuilder>
	where TFurnitureBuilder : IFloorRoomItemBuilder<TFurniture, TTarget, TFurnitureBuilder, TBuilder>;
