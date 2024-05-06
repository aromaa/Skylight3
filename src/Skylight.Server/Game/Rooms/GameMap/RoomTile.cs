using System.Diagnostics;
using Skylight.API.Game.Rooms.Items.Floor;
using Skylight.API.Game.Rooms.Items.Interactions.Wired.Triggers;
using Skylight.API.Game.Rooms.Map;
using Skylight.API.Game.Rooms.Units;
using Skylight.API.Numerics;
using Skylight.Server.Collections;
using Skylight.Server.Game.Rooms.Layout;

namespace Skylight.Server.Game.Rooms.GameMap;

internal sealed class RoomTile : IRoomTile
{
	private readonly Room room;

	public IRoomMap Map { get; }
	public Point3D Position { get; private set; }

	private readonly IntervalTree<double, IFloorRoomItem> heightMap;
	private readonly Dictionary<int, IRoomUnit> roomUnits;

	internal RoomLayoutTile LayoutTile { get; }

	internal RoomTile(Room room, IRoomMap map, Point2D location, RoomLayoutTile layoutTile)
	{
		this.room = room;

		this.Map = map;
		this.Position = new Point3D(location, layoutTile.Height);

		this.heightMap = new IntervalTree<double, IFloorRoomItem>();
		this.roomUnits = [];

		this.LayoutTile = layoutTile;
	}

	public bool IsHole => this.LayoutTile.IsHole;
	public bool HasRoomUnit => this.roomUnits.Count > 0;

	public IEnumerable<IFloorRoomItem> FloorItems => this.heightMap.Values;
	public IEnumerable<IRoomUnit> Units => this.roomUnits.Values;

	public IEnumerable<IFloorRoomItem> GetFloorItemsBetween(double minZ, double maxZ) => this.heightMap.GetItemsBetween(minZ, maxZ);

	public double GetStepHeight(double z) => this.GetStepHeight(z, 2, 2);
	internal double GetStepHeight(double z, double range, double emptySpace)
	{
		if (this.heightMap.TryFindGab(z + range, emptySpace, out double value))
		{
			return value;
		}

		return this.Position.Z;
	}

	public void AddItem(IFloorRoomItem item)
	{
		this.heightMap.Add(item.Position.Z, item.Position.Z + item.Height, item);

		this.Position = new Point3D(this.Position.XY, this.heightMap.Max);
	}

	public void RemoveItem(IFloorRoomItem item)
	{
		this.heightMap.Remove(item.Position.Z, item.Position.Z + item.Height, item);

		this.Position = new Point3D(this.Position.XY, this.heightMap.Count > 0 ? this.heightMap.Max : this.LayoutTile.Height);
	}

	public void WalkOff(IRoomUnit unit)
	{
		bool result = this.roomUnits.Remove(unit.Id);

		Debug.Assert(result);

		IFloorRoomItem? item = this.FloorItems.FirstOrDefault(i => i.Position.Z + i.Height == unit.Position.Z);
		if (item is not null && this.room.ItemManager.TryGetInteractionHandler(out IUnitWalkOffTriggerInteractionHandler? handler))
		{
			handler.OnWalkOff((IUserRoomUnit)unit, item);
		}
	}

	public void WalkOn(IRoomUnit unit)
	{
		bool result = this.roomUnits.TryAdd(unit.Id, unit);

		Debug.Assert(result);

		IFloorRoomItem? item = this.FloorItems.FirstOrDefault(i => i.Position.Z + i.Height == unit.Position.Z);
		if (item is not null && this.room.ItemManager.TryGetInteractionHandler(out IUnitWalkOnTriggerInteractionHandler? handler))
		{
			handler.OnWalkOn((IUserRoomUnit)unit, item);
		}
	}
}
