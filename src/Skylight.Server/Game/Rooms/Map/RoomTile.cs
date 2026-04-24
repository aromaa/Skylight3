using Skylight.API.Game.Furniture.Floor;
using Skylight.API.Game.Rooms.Map;
using Skylight.API.Game.Rooms.Units;
using Skylight.API.Numerics;
using Skylight.Server.Game.Rooms.Layout;

namespace Skylight.Server.Game.Rooms.Map;

internal abstract class RoomTile : IRoomTile
{
	public IRoomMap Map { get; }
	public Point3D Position { get; protected set; }

	internal RoomLayoutTile LayoutTile { get; }

	internal RoomTile(IRoomMap map, Point2D location, RoomLayoutTile layoutTile)
	{
		this.Map = map;
		this.Position = new Point3D(location, layoutTile.Height);

		this.LayoutTile = layoutTile;
	}

	public bool IsHole => this.LayoutTile.IsHole;
	public abstract bool HasRoomUnit { get; }

	public abstract IEnumerable<IRoomUnit> Units { get; }

	public abstract IRoomTileSection? GetSection(double z);

	public abstract IRoomTileSection? FindSection(double z, Func<IFloorFurniture, bool> func);
	internal abstract IRoomTileSection? FindSection(double z, double range, double emptySpace);
}
