using Skylight.API.Game.Rooms.Map;
using Skylight.API.Game.Rooms.Map.Public;
using Skylight.API.Numerics;
using Skylight.Server.Game.Rooms.Layout;

namespace Skylight.Server.Game.Rooms.Map.Public;

internal sealed class PublicRoomTile(IRoomMap map, Point2D location, RoomLayoutTile layoutTile) : RoomTile(map, location, layoutTile), IPublicRoomTile
{
	public override double? GetStepHeight(double z)
	{
		return this.LayoutTile.Height;
	}

	internal override double? GetStepHeight(double z, double range, double emptySpace) => this.LayoutTile.Height;
}
