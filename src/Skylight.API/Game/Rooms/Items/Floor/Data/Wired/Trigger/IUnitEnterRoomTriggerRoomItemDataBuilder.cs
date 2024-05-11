using Skylight.API.Game.Furniture;
using Skylight.API.Game.Furniture.Floor;
using Skylight.API.Game.Furniture.Floor.Wired.Triggers;
using Skylight.API.Game.Inventory.Items.Floor;
using Skylight.API.Game.Rooms.Items.Floor.Wired.Triggers;

namespace Skylight.API.Game.Rooms.Items.Floor.Data.Wired.Trigger;

public interface IUnitEnterRoomTriggerRoomItemDataBuilder : IFurnitureItemDataBuilder<IFurniMaticGiftFurniture, IFurniMaticGiftInventoryItem, IUnitEnterRoomTriggerRoomItemDataBuilder>
{
	public IUnitEnterRoomTriggerRoomItemDataBuilder TriggerUsername(string triggerUsername);

	public new interface IRaw<out TBuilder> : IFurnitureItemDataBuilder.IRaw<TBuilder>, IUnitEnterRoomTriggerRoomItemDataBuilder
		where TBuilder : IRaw<TBuilder>;
}

public interface IUnitEnterRoomTriggerRoomItemDataBuilder<in TFurniture, out TTarget, out TBuilder> : IUnitEnterRoomTriggerRoomItemDataBuilder
	where TFurniture : IUnitEnterRoomTriggerFurniture
	where TTarget : IUnitEnterRoomTriggerRoomItem, IFurnitureItem<TFurniture>
	where TBuilder : IUnitEnterRoomTriggerRoomItemDataBuilder<TFurniture, TTarget, TBuilder>, IFurnitureItemDataBuilder<TFurniture, TTarget, TBuilder>;

public interface IUnitEnterRoomTriggerRoomItemDataBuilder<in TFurniture, out TTarget, out TBuilder, out TFurnitureBuilder> : IUnitEnterRoomTriggerRoomItemDataBuilder<TFurniture, TTarget, TBuilder>, IFloorRoomItemBuilder<TFurniture, TTarget, TFurnitureBuilder, TBuilder>
	where TFurniture : IUnitEnterRoomTriggerFurniture
	where TTarget : IUnitEnterRoomTriggerRoomItem, IFurnitureItem<TFurniture>
	where TBuilder : IUnitEnterRoomTriggerRoomItemDataBuilder<TFurniture, TTarget, TBuilder, TFurnitureBuilder>, IFurnitureItemDataBuilder<TFurniture, TTarget, TBuilder, TFurnitureBuilder>
	where TFurnitureBuilder : IFloorRoomItemBuilder<TFurniture, TTarget, TFurnitureBuilder, TBuilder>;
