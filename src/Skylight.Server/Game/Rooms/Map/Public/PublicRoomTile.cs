using Skylight.API.Game.Furniture.Floor;
using Skylight.API.Game.Rooms.Map;
using Skylight.API.Game.Rooms.Map.Public;
using Skylight.API.Game.Rooms.Units;
using Skylight.API.Numerics;
using Skylight.Server.Game.Rooms.Layout;

namespace Skylight.Server.Game.Rooms.Map.Public;

internal sealed class PublicRoomTile(IRoomMap map, Point2D location, RoomLayoutTile layoutTile) : RoomTile(map, location, layoutTile), IPublicRoomTile
{
	private readonly PublicRoomTileSection section = new(new Point3D(location, layoutTile.Height));

	public override bool HasRoomUnit => this.section.RoomUnits.Count > 0;
	public override IEnumerable<IRoomUnit> Units => this.section.RoomUnits;

	public override IRoomTileSection GetSection(double z) => this.section;
	public override IRoomTileSection FindSection(double z, Func<IFloorFurniture, bool> func) => this.section;
	internal override IRoomTileSection FindSection(double z, double range, double emptySpace) => this.section;
}
