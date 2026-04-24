using Skylight.API.Game.Furniture.Floor;
using Skylight.API.Game.Rooms.Items.Floor;
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

	private readonly IntervalTree<double, PrivateRoomTileSection> heightMap;

	private readonly IFloorFurnitureKind? walkable;

	internal PrivateRoomTile(PrivateRoom room, IRoomMap map, IRegistryHolder registryHolder, Point2D location, RoomLayoutTile layoutTile)
		: base(map, location, layoutTile)
	{
		this.room = room;

		this.heightMap = new IntervalTree<double, PrivateRoomTileSection>();
		this.heightMap.GetOrAdd(layoutTile.Height, layoutTile.Height, p => new PrivateRoomTileSection(p.Tile.room, p.Position), (Tile: this, Position: new Point3D(location, layoutTile.Height)));

		if (FloorFurnitureKindTypes.Walkable.TryGet(registryHolder, out IFloorFurnitureKindType? walkableType))
		{
			this.walkable = walkableType.Value;
		}
	}

	public override bool HasRoomUnit => this.heightMap.Values.Any(i => i.RoomUnits.Count > 0);

	public IEnumerable<IFloorRoomItem> FloorItems => this.heightMap.Values.SelectMany(i => i.Items);
	public override IEnumerable<IRoomUnit> Units => this.heightMap.Values.SelectMany(i => i.RoomUnits);

	public IEnumerable<IFloorRoomItem> GetFloorItemsBetween(double minZ, double maxZ) => this.heightMap.GetItemsBetween(minZ, maxZ).SelectMany(i => i.Items);

	public override IRoomTileSection? FindSection(double z, Func<IFloorFurniture, bool> func) => this.FindSection(z, 2, 2, func);

	internal override IRoomTileSection? FindSection(double z, double range, double emptySpace) => this.FindSection(z, range, emptySpace, f => f.Kind == this.walkable);

	public override IRoomTileSection? GetSection(double z) => this.heightMap.Get(z);

	private IRoomTileSection? FindSection(double z, double range, double emptySpace, Func<IFloorFurniture, bool> func)
	{
		if (this.heightMap.FindGabGreedy(z + range, emptySpace, s => s.Items.Count <= 0 || func(s.Items.First().Furniture), out PrivateRoomTileSection? item))
		{
			return item;
		}

		return null;
	}

	public void AddItem(IFloorRoomItem item)
	{
		PrivateRoomTileSection section = this.heightMap.GetOrAdd(item.Position.Z, item.Position.Z + item.Height, static p => new PrivateRoomTileSection(p.Tile.room, p.Position), (Tile: this, Position: new Point3D(this.Position.XY, item.Position.Z + item.Height)));
		section.Items.Add(item);

		this.Position = new Point3D(this.Position.XY, this.heightMap.Max);
	}

	public void RemoveItem(IFloorRoomItem item)
	{
		this.heightMap.RemoveIf(item.Position.Z, item.Position.Z + item.Height, (s, i) => s.Items.Remove(i) && s is { Floor: false, Items.Count: <= 0 }, item);

		this.Position = new Point3D(this.Position.XY, this.heightMap.Count > 0 ? this.heightMap.Max : this.LayoutTile.Height);
	}
}
