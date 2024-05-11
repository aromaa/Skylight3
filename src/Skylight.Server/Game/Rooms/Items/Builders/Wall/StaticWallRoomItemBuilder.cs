using Skylight.API.Game.Furniture.Wall;
using Skylight.API.Game.Rooms.Items.Wall;
using Skylight.Server.Game.Rooms.Items.Wall;

namespace Skylight.Server.Game.Rooms.Items.Builders.Wall;

internal sealed class StaticWallRoomItemBuilder : WallRoomItemBuilder<IStaticWallFurniture, IStaticWallRoomItem, StaticWallRoomItemBuilder>
{
	public override IStaticWallRoomItem Build()
	{
		this.CheckValid();

		return new StaticWallRoomItem(this.RoomValue, this.IdValue, this.OwnerValue, this.FurnitureValue, this.LocationValue, this.PositionValue, 0);
	}
}
