using System.Diagnostics;
using Skylight.API.Game.Rooms.Items.Floor;
using Skylight.API.Game.Rooms.Map;
using Skylight.API.Game.Rooms.Units;
using Skylight.API.Numerics;
using Skylight.Server.Collections;
using Skylight.Server.Game.Rooms.Items;
using Skylight.Server.Game.Rooms.Layout;

namespace Skylight.Server.Game.Rooms.GameMap;

internal sealed class RoomTile : IRoomTile
{
	public IRoomMap Map { get; }

	private readonly RangeMap<double, IFloorRoomItem> heightMap;

	private readonly Dictionary<int, IRoomUnit> roomUnits;

	private Point3D position;
	public Point3D Position => this.position;

	internal RoomLayoutTile LayoutTile { get; }

	internal RoomTile(IRoomMap map, Point2D location, RoomLayoutTile layoutTile)
	{
		this.Map = map;

		this.heightMap = new RangeMap<double, IFloorRoomItem>(RoomItemHeightComparer.Instance);

		this.roomUnits = new Dictionary<int, IRoomUnit>();

		this.position = new Point3D(location, layoutTile.Height);

		this.LayoutTile = layoutTile;
	}

	public bool IsHole => this.LayoutTile.IsHole;
	public bool HasRoomUnit => this.roomUnits.Count > 0;

	public IEnumerable<IFloorRoomItem> FloorItems => this.heightMap.Values;
	public IEnumerable<IRoomUnit> Units => this.roomUnits.Values;

	public IEnumerable<IFloorRoomItem> GetFloorItemsBetween(double minZ, double maxZ) => this.heightMap.GetViewBetween(minZ, maxZ);

	public double GetStepHeight(double z) => this.GetStepHeight(z, 2, 2);
	internal double GetStepHeight(double z, double range, double emptySpace)
	{
		SortedSet<IFloorRoomItem>? values = this.heightMap.FindNearestValues(z, range, emptySpace);
		if (values?.Max is { } topItem)
		{
			return topItem.Position.Z + topItem.Furniture.Height;
		}

		return this.position.Z;
	}

	public void AddItem(IFloorRoomItem item)
	{
		this.heightMap.Add(item.Position.Z, item.Position.Z + item.Furniture.Height, item);

		this.position.Z = this.heightMap.Max is { } highestItem ? highestItem.Position.Z + highestItem.Furniture.Height : this.LayoutTile.Height;
	}

	public void RemoveItem(IFloorRoomItem item)
	{
		this.heightMap.Remove(item.Position.Z, item.Position.Z + item.Furniture.Height, item);

		this.position.Z = this.heightMap.Max is { } highestItem ? highestItem.Position.Z + highestItem.Furniture.Height : this.LayoutTile.Height;
	}

	public void WalkOff(IRoomUnit unit)
	{
		bool result = this.roomUnits.Remove(unit.Id);

		Debug.Assert(result);
	}

	public void WalkOn(IRoomUnit unit)
	{
		bool result = this.roomUnits.TryAdd(unit.Id, unit);

		Debug.Assert(result);
	}
}
