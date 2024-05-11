using Skylight.API.Game.Furniture;
using Skylight.API.Game.Furniture.Floor;
using Skylight.API.Game.Furniture.Floor.Wired.Effects;
using Skylight.API.Game.Inventory.Items.Floor;
using Skylight.API.Game.Rooms.Items.Floor.Wired.Effects;

namespace Skylight.API.Game.Rooms.Items.Floor.Data.Wired.Effect;

public interface IWiredEffectRoomItemDataBuilder : IFurnitureItemDataBuilder<IFurniMaticGiftFurniture, IFurniMaticGiftInventoryItem, IWiredEffectRoomItemDataBuilder>
{
	public IWiredEffectRoomItemDataBuilder EffectDelay(int effectDelay);

	public new interface IRaw<out TBuilder> : IFurnitureItemDataBuilder.IRaw<TBuilder>, IWiredEffectRoomItemDataBuilder
		where TBuilder : IRaw<TBuilder>;
}

public interface IWiredEffectRoomItemDataBuilder<in TFurniture, out TTarget, out TBuilder> : IWiredEffectRoomItemDataBuilder
	where TFurniture : IWiredEffectFurniture
	where TTarget : IWiredEffectRoomItem, IFurnitureItem<TFurniture>
	where TBuilder : IWiredEffectRoomItemDataBuilder<TFurniture, TTarget, TBuilder>, IFurnitureItemDataBuilder<TFurniture, TTarget, TBuilder>;

public interface IWiredEffectRoomItemDataBuilder<in TFurniture, out TTarget, out TBuilder, out TFurnitureBuilder> : IWiredEffectRoomItemDataBuilder<TFurniture, TTarget, TBuilder>
	where TFurniture : IWiredEffectFurniture
	where TTarget : IWiredEffectRoomItem, IFurnitureItem<TFurniture>
	where TBuilder : IWiredEffectRoomItemDataBuilder<TFurniture, TTarget, TBuilder, TFurnitureBuilder>, IFurnitureItemDataBuilder<TFurniture, TTarget, TBuilder, TFurnitureBuilder>
	where TFurnitureBuilder : IFloorRoomItemBuilder<TFurniture, TTarget, TFurnitureBuilder, TBuilder>;
