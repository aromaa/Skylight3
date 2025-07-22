using Skylight.API.Game.Furniture;
using Skylight.API.Game.Furniture.Floor;
using Skylight.API.Game.Furniture.Floor.Wired.Effects;
using Skylight.API.Game.Inventory.Items.Floor;
using Skylight.API.Game.Rooms.Items.Floor.Wired.Effects;

namespace Skylight.API.Game.Rooms.Items.Floor.Data.Wired.Effect;

public interface IShowMessageEffectRoomItemDataBuilder : IFurnitureItemDataBuilder<IFurniMaticGiftFurniture, IFurniMaticGiftInventoryItem, IShowMessageEffectRoomItemDataBuilder>
{
	public IShowMessageEffectRoomItemDataBuilder Message(string message);

	public new interface IRaw<out TBuilder> : IFurnitureItemDataBuilder.IRaw<TBuilder>, IShowMessageEffectRoomItemDataBuilder
		where TBuilder : IRaw<TBuilder>;
}

public interface IShowMessageEffectRoomItemDataBuilder<in TFurniture, out TTarget, out TBuilder> : IShowMessageEffectRoomItemDataBuilder
	where TFurniture : IShowMessageEffectFurniture
	where TTarget : IShowMessageEffectRoomItem, IFurnitureItem<TFurniture>
	where TBuilder : IShowMessageEffectRoomItemDataBuilder<TFurniture, TTarget, TBuilder>, IFurnitureItemDataBuilder<TFurniture, TTarget, TBuilder>, IWiredEffectRoomItemDataBuilder<TFurniture, TTarget, TBuilder>;

public interface IShowMessageEffectRoomItemDataBuilder<in TFurniture, out TTarget, out TBuilder, out TFurnitureBuilder> : IShowMessageEffectRoomItemDataBuilder<TFurniture, TTarget, TBuilder>, IFloorRoomItemBuilder<TFurniture, TTarget, TFurnitureBuilder, TBuilder>
	where TFurniture : IShowMessageEffectFurniture
	where TTarget : IShowMessageEffectRoomItem, IFurnitureItem<TFurniture>
	where TBuilder : IShowMessageEffectRoomItemDataBuilder<TFurniture, TTarget, TBuilder, TFurnitureBuilder>, IFurnitureItemDataBuilder<TFurniture, RoomItemId, TTarget, TBuilder, TFurnitureBuilder>, IWiredEffectRoomItemDataBuilder<TFurniture, TTarget, TBuilder, TFurnitureBuilder>
	where TFurnitureBuilder : IFloorRoomItemBuilder<TFurniture, TTarget, TFurnitureBuilder, TBuilder>;
