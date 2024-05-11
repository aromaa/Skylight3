using Skylight.API.Game.Furniture.Floor;
using Skylight.API.Game.Rooms.Items.Floor;
using Skylight.API.Game.Rooms.Items.Interactions;
using Skylight.Server.Game.Rooms.Items.Floor;

namespace Skylight.Server.Game.Rooms.Items.Builders.Floor;

internal sealed class RollerRoomItemBuilder : FloorItemBuilder<IRollerFurniture, IRollerRoomItem, RollerRoomItemBuilder>
{
	public override IRollerRoomItem Build()
	{
		this.CheckValid();

		if (!this.RoomValue.ItemManager.TryGetInteractionHandler(out IRollerInteractionHandler? handler))
		{
			throw new Exception($"{typeof(IRollerInteractionHandler)} not found");
		}

		return new RollerRoomItem(this.RoomValue, this.IdValue, this.OwnerValue, this.FurnitureValue, this.PositionValue, this.DirectionValue, handler);
	}
}
