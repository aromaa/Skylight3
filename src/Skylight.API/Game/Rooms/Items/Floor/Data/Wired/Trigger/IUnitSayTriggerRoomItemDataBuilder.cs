using Skylight.API.Game.Furniture;
using Skylight.API.Game.Furniture.Floor;
using Skylight.API.Game.Furniture.Floor.Wired.Triggers;
using Skylight.API.Game.Inventory.Items.Floor;
using Skylight.API.Game.Rooms.Items.Floor.Wired.Triggers;

namespace Skylight.API.Game.Rooms.Items.Floor.Data.Wired.Trigger;

public interface IUnitSayTriggerRoomItemDataBuilder : IFurnitureItemDataBuilder<IFurniMaticGiftFurniture, IFurniMaticGiftInventoryItem, IUnitSayTriggerRoomItemDataBuilder>
{
	public IUnitSayTriggerRoomItemDataBuilder Message(string message);

	public new interface IRaw<out TBuilder> : IFurnitureItemDataBuilder.IRaw<TBuilder>, IUnitSayTriggerRoomItemDataBuilder
		where TBuilder : IRaw<TBuilder>;
}

public interface IUnitSayTriggerRoomItemDataBuilder<in TFurniture, out TTarget, out TBuilder> : IUnitSayTriggerRoomItemDataBuilder
	where TFurniture : IUnitSayTriggerFurniture
	where TTarget : IUnitSayTriggerRoomItem, IFurnitureItem<TFurniture>
	where TBuilder : IUnitSayTriggerRoomItemDataBuilder<TFurniture, TTarget, TBuilder>, IFurnitureItemDataBuilder<TFurniture, TTarget, TBuilder>;

public interface IUnitSayTriggerRoomItemDataBuilder<in TFurniture, out TTarget, out TBuilder, out TFurnitureBuilder> : IUnitSayTriggerRoomItemDataBuilder<TFurniture, TTarget, TBuilder>, IFloorRoomItemBuilder<TFurniture, TTarget, TFurnitureBuilder, TBuilder>
	where TFurniture : IUnitSayTriggerFurniture
	where TTarget : IUnitSayTriggerRoomItem, IFurnitureItem<TFurniture>
	where TBuilder : IUnitSayTriggerRoomItemDataBuilder<TFurniture, TTarget, TBuilder, TFurnitureBuilder>, IFurnitureItemDataBuilder<TFurniture, TTarget, TBuilder, TFurnitureBuilder>
	where TFurnitureBuilder : IFloorRoomItemBuilder<TFurniture, TTarget, TFurnitureBuilder, TBuilder>;
