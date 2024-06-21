using Skylight.API.Game.Furniture.Floor;
using Skylight.API.Game.Rooms.Items.Floor;
using Skylight.Server.Game.Rooms.Items.Floor;

namespace Skylight.Server.Game.Rooms.Items.Builders.Floor;

internal sealed class BasicFloorRoomItemBuilder : FloorItemBuilder<IBasicFloorFurniture, IBasicFloorRoomItem, BasicFloorRoomItemBuilder>
{
	public override IBasicFloorRoomItem Build()
	{
		this.CheckValid();

		return new BasicFloorRoomItem(this.RoomValue, this.IdValue, this.OwnerValue, this.FurnitureValue, this.PositionValue, this.DirectionValue, this.ExtraDataValue?.RootElement.GetInt32() ?? 0);
	}
}
