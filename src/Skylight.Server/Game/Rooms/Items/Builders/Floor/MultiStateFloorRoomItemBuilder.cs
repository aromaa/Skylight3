using Skylight.API.Game.Furniture.Floor;
using Skylight.API.Game.Rooms.Items.Floor;
using Skylight.Server.Game.Rooms.Items.Floor;

namespace Skylight.Server.Game.Rooms.Items.Builders.Floor;

internal sealed class MultiStateFloorRoomItemBuilder : FloorItemBuilder<IMultiStateFloorFurniture, IMultiStateFloorItem, MultiStateFloorRoomItemBuilder>
{
	public override IMultiStateFloorItem Build()
	{
		this.CheckValid();

		return new MultiStateFloorRoomItem<IMultiStateFloorFurniture>(this.RoomValue, this.IdValue, this.OwnerValue, this.FurnitureValue, this.PositionValue, this.DirectionValue);
	}
}
