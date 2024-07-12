using System.Diagnostics;
using Skylight.API.Game.Rooms.Map;
using Skylight.API.Game.Rooms.Units;
using Skylight.API.Numerics;
using Skylight.Server.Game.Rooms.Layout;

namespace Skylight.Server.Game.Rooms.Map;

internal abstract class RoomTile : IRoomTile
{
	public IRoomMap Map { get; }
	public Point3D Position { get; protected set; }

	private readonly Dictionary<int, IRoomUnit> roomUnits;

	internal RoomLayoutTile LayoutTile { get; }

	internal RoomTile(IRoomMap map, Point2D location, RoomLayoutTile layoutTile)
	{
		this.Map = map;
		this.Position = new Point3D(location, layoutTile.Height);

		this.roomUnits = [];

		this.LayoutTile = layoutTile;
	}

	public bool IsHole => this.LayoutTile.IsHole;
	public bool HasRoomUnit => this.roomUnits.Count > 0;

	public IEnumerable<IRoomUnit> Units => this.roomUnits.Values;

	public abstract double? GetStepHeight(double z);
	internal abstract double? GetStepHeight(double z, double range, double emptySpace);

	public virtual void WalkOff(IRoomUnit unit)
	{
		bool result = this.roomUnits.Remove(unit.Id);

		Debug.Assert(result);
	}

	public virtual void WalkOn(IRoomUnit unit)
	{
		bool result = this.roomUnits.TryAdd(unit.Id, unit);

		Debug.Assert(result);
	}
}
