using Skylight.API.Game.Furniture.Floor;
using Skylight.API.Game.Rooms.Items.Floor;
using Skylight.API.Game.Rooms.Items.Interactions.Wired.Triggers;
using Skylight.API.Game.Rooms.Map;
using Skylight.API.Game.Rooms.Map.Private;
using Skylight.API.Game.Rooms.Units;
using Skylight.API.Numerics;
using Skylight.API.Registry;
using Skylight.Server.Collections;
using Skylight.Server.Game.Rooms.Layout;
using Skylight.Server.Game.Rooms.Private;

namespace Skylight.Server.Game.Rooms.Map.Private;

internal sealed class PrivateRoomTile : RoomTile, IPrivateRoomTile
{
	private readonly PrivateRoom room;

	private readonly IntervalTree<double, IFloorRoomItem> heightMap;

	private readonly IFloorFurnitureKind? walkable;

	internal PrivateRoomTile(PrivateRoom room, IRoomMap map, IRegistryHolder registryHolder, Point2D location, RoomLayoutTile layoutTile)
		: base(map, location, layoutTile)
	{
		this.room = room;

		this.heightMap = new IntervalTree<double, IFloorRoomItem>();

		if (FloorFurnitureKindTypes.Walkable.TryGet(registryHolder, out IFloorFurnitureKindType? walkableType))
		{
			this.walkable = walkableType.Value;
		}
	}

	public IEnumerable<IFloorRoomItem> FloorItems => this.heightMap.Values;

	public IEnumerable<IFloorRoomItem> GetFloorItemsBetween(double minZ, double maxZ) => this.heightMap.GetItemsBetween(minZ, maxZ);

	public override double? GetStepHeight(double z) => this.GetStepHeight(z, 2, 2);
	internal override double? GetStepHeight(double z, double range, double emptySpace)
	{
		switch (this.heightMap.FindGabGreedy(z + range, emptySpace, item => item.Furniture.Kind == this.walkable, out double value))
		{
			case IntervalTree<double, IFloorRoomItem>.SearchResult.Success:
				return value;
			case IntervalTree<double, IFloorRoomItem>.SearchResult.Fallback:
				if (value - this.LayoutTile.Height < 0)
				{
					return null;
				}

				goto default;
			default:
				return this.LayoutTile.Height;
		}
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

	public override void WalkOff(IRoomUnit unit)
	{
		base.WalkOff(unit);

		IFloorRoomItem? item = this.FloorItems.FirstOrDefault(i => i.Position.Z + i.Height == unit.Position.Z);
		if (item is not null && this.room.ItemManager.TryGetInteractionHandler(out IUnitWalkOffTriggerInteractionHandler? handler))
		{
			handler.OnWalkOff((IUserRoomUnit)unit, item);
		}
	}

	public override void WalkOn(IRoomUnit unit)
	{
		base.WalkOn(unit);

		IFloorRoomItem? item = this.FloorItems.FirstOrDefault(i => i.Position.Z + i.Height == unit.Position.Z);
		if (item is not null && this.room.ItemManager.TryGetInteractionHandler(out IUnitWalkOnTriggerInteractionHandler? handler))
		{
			handler.OnWalkOn((IUserRoomUnit)unit, item);
		}
	}
}
