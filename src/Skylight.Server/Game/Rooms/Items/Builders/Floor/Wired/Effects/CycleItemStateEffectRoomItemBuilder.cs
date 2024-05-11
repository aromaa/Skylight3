using Skylight.API.Game.Furniture.Floor.Wired.Effects;
using Skylight.API.Game.Rooms.Items.Floor.Wired.Effects;
using Skylight.API.Game.Rooms.Items.Interactions.Wired.Effects;
using Skylight.Server.Game.Rooms.Items.Floor.Wired.Effects;

namespace Skylight.Server.Game.Rooms.Items.Builders.Floor.Wired.Effects;

internal sealed class CycleItemStateEffectRoomItemBuilder : WiredEffectRoomItemBuilder<ICycleItemStateEffectFurniture, ICycleItemStateRoomItem, CycleItemStateEffectRoomItemBuilder, CycleItemStateEffectRoomItemBuilder>
{
	protected override ICycleItemStateRoomItem Build(int effectDelay)
	{
		this.CheckValid();

		if (!this.RoomValue.ItemManager.TryGetInteractionHandler(out IWiredEffectInteractionHandler? handler))
		{
			throw new Exception($"{typeof(IWiredEffectInteractionHandler)} not found");
		}

		return new CycleItemStateEffectRoomItem(this.RoomValue, this.IdValue, this.OwnerValue, this.FurnitureValue, this.PositionValue, this.DirectionValue, handler, null, this.ExtraDataValue, effectDelay);
	}
}
