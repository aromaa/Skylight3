using Skylight.API.Game.Furniture.Floor;
using Skylight.API.Game.Rooms.Items.Floor;
using Skylight.Server.Game.Rooms.Items.Floor;

namespace Skylight.Server.Game.Rooms.Items.Builders.Floor;

internal sealed class VariableHeightRoomItemBuilder : FloorItemBuilder<IVariableHeightFurniture, IVariableHeightRoomItem, VariableHeightRoomItemBuilder>
{
	public override IVariableHeightRoomItem Build()
	{
		this.CheckValid();

		return new VariableHeightRoomItem(this.RoomValue, this.IdValue, this.OwnerValue, this.FurnitureValue, this.PositionValue, this.DirectionValue, this.ExtraDataValue?.RootElement.GetInt32() ?? 0);
	}
}
