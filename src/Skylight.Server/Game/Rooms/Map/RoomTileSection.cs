using System.Diagnostics;
using Skylight.API.Game.Rooms.Map;
using Skylight.API.Game.Rooms.Units;
using Skylight.API.Numerics;

namespace Skylight.Server.Game.Rooms.Map;

internal abstract class RoomTileSection(Point3D position) : IRoomTileSection
{
	private readonly Dictionary<int, IRoomUnit> roomUnits = [];

	public Point3D Position { get; } = position;

	internal ICollection<IRoomUnit> RoomUnits => this.roomUnits.Values;

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
