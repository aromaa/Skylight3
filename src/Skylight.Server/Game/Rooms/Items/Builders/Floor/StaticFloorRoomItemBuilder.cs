using Skylight.API.Game.Furniture.Floor;
using Skylight.API.Game.Rooms.Items.Floor;
using Skylight.Server.Game.Rooms.Items.Floor;

namespace Skylight.Server.Game.Rooms.Items.Builders.Floor;

internal sealed class StaticFloorRoomItemBuilder : FloorItemBuilder<IStaticFloorFurniture, IStaticFloorRoomItem, StaticFloorRoomItemBuilder>
{
	public override IStaticFloorRoomItem Build()
	{
		this.CheckValid();

		return new StaticFloorRoomItem(this.RoomValue, this.IdValue, this.OwnerValue, this.FurnitureValue, this.PositionValue, this.DirectionValue);
	}
}
