using System.Diagnostics;
using Skylight.API.Game.Rooms.Items.Floor;
using Skylight.API.Game.Rooms.Map;
using Skylight.API.Game.Rooms.Units;
using Skylight.API.Numerics;
using Skylight.Server.Collections;
using Skylight.Server.Game.Rooms.Layout;

namespace Skylight.Server.Game.Rooms.GameMap;

internal sealed class RoomTile : IRoomTile
{
	private readonly RangeMap<double, IFloorRoomItem> heightMap;

	private readonly Dictionary<int, IRoomUnit> roomUnits;

	private Point3D position;
	public Point3D Position => this.position;

	internal RoomLayoutTile LayoutTile { get; }

	internal RoomTile(Point2D location, RoomLayoutTile layoutTile)
	{
		this.heightMap = new RangeMap<double, IFloorRoomItem>(SortItemsByZAndHeight.Instance);

		this.roomUnits = new Dictionary<int, IRoomUnit>();

		this.position = new Point3D(location, layoutTile.Height);

		this.LayoutTile = layoutTile;
	}

	public bool IsHole => this.LayoutTile.IsHole;
	public bool HasRoomUnit => this.roomUnits.Count > 0;

	public double GetStepHeight(double z)
	{
		SortedSet<IFloorRoomItem>? values = this.heightMap.FindNearestValues(z, 2, 2);
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

	private sealed class SortItemsByZAndHeight : IComparer<IFloorRoomItem>
	{
		internal static readonly SortItemsByZAndHeight Instance = new();

		public int Compare(IFloorRoomItem? x, IFloorRoomItem? y)
		{
			//Item with highest Z
			int result = x!.Position.Z.CompareTo(y!.Position.Z);
			if (result != 0)
			{
				return result;
			}

			//Item with highest height
			result = x.Furniture.Height.CompareTo(y.Furniture.Height);
			if (result != 0)
			{
				return result;
			}

			//Last resort, the most recently purchased item
			return x.Id.CompareTo(y.Id);
		}
	}
}
